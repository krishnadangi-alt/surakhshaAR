"""
Automated tests for ML Competency Engine.

Tests cover:
- Scoring engine basic functionality
- Critical error handling (FAIL override)
- Weakness detection
- Retraining recommendations
"""

import pytest
from ml.competency.scoring import CompetencyScorer, ScoringResult
from ml.competency.weakness_detection import WeaknessDetector
from ml.competency.retraining import RetrainingRecommender
from ml.competency.sample_data import (
    get_sample_fire_assessment,
    FIRE_ASSESSMENT_GOOD,
    FIRE_ASSESSMENT_POOR,
    FIRE_ASSESSMENT_CRITICAL_ERROR,
)


class TestScoringEngine:
    """Tests for the competency scoring engine."""
    
    def test_scorer_initialization(self):
        """Test that scorer initializes correctly."""
        scorer = CompetencyScorer(scenario_type="fire")
        assert scorer.scenario_type == "fire"
        assert len(scorer.competencies) > 0
    
    def test_good_assessment_passes(self):
        """Test that good assessment results in PASS."""
        scorer = CompetencyScorer(scenario_type="fire")
        
        # Process events from good assessment
        for event in FIRE_ASSESSMENT_GOOD["events"]:
            scorer.process_event(event)
        
        result = scorer.get_result()
        
        assert result.passed is True, "Good assessment should pass"
        assert result.overall_score >= 70.0, "Overall score should be adequate"
        assert len(result.critical_errors) == 0, "Good assessment should have no critical errors"
    
    def test_poor_assessment_fails(self):
        """Test that poor assessment results in FAIL."""
        scorer = CompetencyScorer(scenario_type="fire")
        
        # Process events from poor assessment
        for event in FIRE_ASSESSMENT_POOR["events"]:
            scorer.process_event(event)
        
        result = scorer.get_result()
        
        assert result.passed is False, "Poor assessment should fail"
        # Should fail due to low scores, not critical errors
        assert len(result.critical_errors) == 0 or result.overall_score < 70.0
    
    def test_critical_error_causes_fail(self):
        """Test that critical error causes automatic FAIL even with high scores."""
        scorer = CompetencyScorer(scenario_type="fire")
        
        # Process events from assessment with critical error
        for event in FIRE_ASSESSMENT_CRITICAL_ERROR["events"]:
            scorer.process_event(event)
        
        result = scorer.get_result()
        
        assert result.passed is False, "Assessment with critical error must FAIL"
        assert len(result.critical_errors) > 0, "Should have recorded critical errors"
        assert "CRITICAL" in result.pass_reason, "Pass reason should mention critical error"
    
    def test_event_processing(self):
        """Test that events are processed and logged."""
        scorer = CompetencyScorer(scenario_type="fire")
        
        event = {
            "event_type": "hazard_identified",
            "correct": True,
            "hazard_type": "fire_source"
        }
        
        scorer.process_event(event)
        result = scorer.get_result()
        
        assert result.events_processed >= 1, "Event should be logged"
    
    def test_scoring_result_serialization(self):
        """Test that scoring result can be serialized to dict."""
        scorer = CompetencyScorer(scenario_type="fire")
        scorer.process_event({
            "event_type": "hazard_identified",
            "correct": True,
            "hazard_type": "fire_source"
        })
        
        result = scorer.get_result()
        result_dict = result.to_dict()
        
        assert isinstance(result_dict, dict)
        assert "assessment_id" in result_dict
        assert "competency_scores" in result_dict
        assert "overall_score" in result_dict
        assert "passed" in result_dict
    
    def test_all_competencies_initialized(self):
        """Test that all fire competencies are properly initialized."""
        scorer = CompetencyScorer(scenario_type="fire")
        result = scorer.get_result()
        
        expected_competencies = {
            "hazard_identification",
            "ppe_selection",
            "procedure_compliance",
            "equipment_use",
            "decision_making"
        }
        
        actual_competencies = set(result.competency_scores.keys())
        assert expected_competencies == actual_competencies, \
            f"Expected {expected_competencies}, got {actual_competencies}"


class TestWeaknessDetection:
    """Tests for weakness detection engine."""
    
    def test_detector_initialization(self):
        """Test that detector initializes correctly."""
        detector = WeaknessDetector()
        assert detector is not None
    
    def test_detects_weak_competencies(self):
        """Test that detector identifies weak competencies."""
        scorer = CompetencyScorer(scenario_type="fire")
        
        # Create poor performance
        for event in FIRE_ASSESSMENT_POOR["events"]:
            scorer.process_event(event)
        
        result = scorer.get_result()
        detector = WeaknessDetector()
        weaknesses = detector.detect_weaknesses(result)
        
        assert len(weaknesses) > 0, "Should detect weaknesses in poor assessment"
        assert all(w.score < w.threshold for w in weaknesses), \
            "All weaknesses should have scores below threshold"
    
    def test_good_assessment_no_weaknesses(self):
        """Test that good assessment has minimal/no weaknesses."""
        scorer = CompetencyScorer(scenario_type="fire")
        
        for event in FIRE_ASSESSMENT_GOOD["events"]:
            scorer.process_event(event)
        
        result = scorer.get_result()
        detector = WeaknessDetector()
        weaknesses = detector.detect_weaknesses(result)
        
        # Good assessment might have minor weaknesses but should be minimal
        assert len(weaknesses) <= 2, "Good assessment should have few/no weaknesses"
    
    def test_weakness_severity_classification(self):
        """Test that weaknesses are classified by severity."""
        scorer = CompetencyScorer(scenario_type="fire")
        
        # Create very poor performance by not doing anything
        scorer.get_result()  # No events processed
        result = scorer.get_result()
        
        detector = WeaknessDetector()
        weaknesses = detector.detect_weaknesses(result)
        
        if weaknesses:
            severities = {w.severity for w in weaknesses}
            # Should have some classification of severity
            assert any(s in severities for s in ["severe", "moderate", "mild"])
    
    def test_weakness_data_structure(self):
        """Test that weakness objects have required fields."""
        scorer = CompetencyScorer(scenario_type="fire")
        
        for event in FIRE_ASSESSMENT_POOR["events"]:
            scorer.process_event(event)
        
        result = scorer.get_result()
        detector = WeaknessDetector()
        weaknesses = detector.detect_weaknesses(result)
        
        for weakness in weaknesses:
            assert hasattr(weakness, "competency_name")
            assert hasattr(weakness, "score")
            assert hasattr(weakness, "threshold")
            assert hasattr(weakness, "severity")
            assert hasattr(weakness, "reason")


class TestRetrainingRecommendation:
    """Tests for targeted retraining recommendations."""
    
    def test_recommender_initialization(self):
        """Test that recommender initializes correctly."""
        recommender = RetrainingRecommender(scenario_type="fire")
        assert recommender.scenario_type == "fire"
        assert recommender.curriculum is not None
    
    def test_recommends_modules_for_weaknesses(self):
        """Test that recommender suggests modules for weak competencies."""
        scorer = CompetencyScorer(scenario_type="fire")
        
        for event in FIRE_ASSESSMENT_POOR["events"]:
            scorer.process_event(event)
        
        result = scorer.get_result()
        detector = WeaknessDetector()
        weaknesses = detector.detect_weaknesses(result)
        
        recommender = RetrainingRecommender(scenario_type="fire")
        modules = recommender.recommend_retraining(weaknesses, max_modules=3)
        
        assert len(modules) > 0, "Should recommend modules for weaknesses"
        assert len(modules) <= 3, "Should respect max_modules parameter"
    
    def test_no_recommendation_for_good_assessment(self):
        """Test that good assessments get minimal retraining."""
        scorer = CompetencyScorer(scenario_type="fire")
        
        for event in FIRE_ASSESSMENT_GOOD["events"]:
            scorer.process_event(event)
        
        result = scorer.get_result()
        detector = WeaknessDetector()
        weaknesses = detector.detect_weaknesses(result)
        
        recommender = RetrainingRecommender(scenario_type="fire")
        modules = recommender.recommend_retraining(weaknesses)
        
        # Good assessment should have minimal/no recommendations
        assert len(modules) <= 2, "Good assessment should need minimal retraining"
    
    def test_retraining_plan_generation(self):
        """Test that recommender generates complete retraining plan."""
        scorer = CompetencyScorer(scenario_type="fire")
        
        for event in FIRE_ASSESSMENT_POOR["events"]:
            scorer.process_event(event)
        
        result = scorer.get_result()
        detector = WeaknessDetector()
        weaknesses = detector.detect_weaknesses(result)
        
        recommender = RetrainingRecommender(scenario_type="fire")
        plan = recommender.get_retraining_plan(weaknesses)
        
        assert "recommended_modules" in plan
        assert "total_estimated_duration_minutes" in plan
        assert isinstance(plan["total_estimated_duration_minutes"], int)
    
    def test_module_data_structure(self):
        """Test that recommended modules have required fields."""
        scorer = CompetencyScorer(scenario_type="fire")
        
        for event in FIRE_ASSESSMENT_POOR["events"]:
            scorer.process_event(event)
        
        result = scorer.get_result()
        detector = WeaknessDetector()
        weaknesses = detector.detect_weaknesses(result)
        
        recommender = RetrainingRecommender(scenario_type="fire")
        modules = recommender.recommend_retraining(weaknesses)
        
        for module in modules:
            assert hasattr(module, "module_id")
            assert hasattr(module, "name")
            assert hasattr(module, "description")
            assert hasattr(module, "estimated_duration_minutes")
            assert hasattr(module, "difficulty_level")
            assert hasattr(module, "reason")


class TestIntegration:
    """Integration tests for complete workflow."""
    
    def test_end_to_end_good_assessment(self):
        """Test complete workflow for good assessment."""
        # Score
        scorer = CompetencyScorer(scenario_type="fire")
        for event in FIRE_ASSESSMENT_GOOD["events"]:
            scorer.process_event(event)
        result = scorer.get_result()
        
        # Should pass
        assert result.passed is True
        
        # Detect weaknesses
        detector = WeaknessDetector()
        weaknesses = detector.detect_weaknesses(result)
        
        # Get retraining
        recommender = RetrainingRecommender(scenario_type="fire")
        plan = recommender.get_retraining_plan(weaknesses)
        
        # Good assessment should have minimal retraining
        assert plan["total_estimated_duration_minutes"] < 60
    
    def test_end_to_end_poor_assessment(self):
        """Test complete workflow for poor assessment."""
        # Score
        scorer = CompetencyScorer(scenario_type="fire")
        for event in FIRE_ASSESSMENT_POOR["events"]:
            scorer.process_event(event)
        result = scorer.get_result()
        
        # Should fail
        assert result.passed is False
        
        # Detect weaknesses
        detector = WeaknessDetector()
        weaknesses = detector.detect_weaknesses(result)
        
        # Should have multiple weaknesses
        assert len(weaknesses) > 0
        
        # Get retraining
        recommender = RetrainingRecommender(scenario_type="fire")
        plan = recommender.get_retraining_plan(weaknesses)
        
        # Should recommend modules
        assert len(plan["recommended_modules"]) > 0
    
    def test_end_to_end_critical_error(self):
        """Test complete workflow for critical error."""
        # Score
        scorer = CompetencyScorer(scenario_type="fire")
        for event in FIRE_ASSESSMENT_CRITICAL_ERROR["events"]:
            scorer.process_event(event)
        result = scorer.get_result()
        
        # Must fail due to critical error
        assert result.passed is False
        assert len(result.critical_errors) > 0
        
        # Detect weaknesses
        detector = WeaknessDetector()
        weaknesses = detector.detect_weaknesses(result)
        
        # Get retraining
        recommender = RetrainingRecommender(scenario_type="fire")
        plan = recommender.get_retraining_plan(weaknesses)
        
        # Should be marked as critical in results
        assert "CRITICAL" in result.pass_reason


if __name__ == "__main__":
    pytest.main([__file__, "-v"])
