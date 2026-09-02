"""
Targeted retraining recommendation engine.

Maps detected weaknesses to specific training modules for targeted learning.
"""

from dataclasses import dataclass
from typing import List, Dict
from ..weakness_detection import Weakness


@dataclass
class TrainingModule:
    """A recommended training module."""
    module_id: str
    name: str
    description: str
    estimated_duration_minutes: int
    difficulty_level: str  # beginner, intermediate, advanced
    competencies_addressed: List[str]
    reason: str  # Why this was recommended


# Mapping from competency weaknesses to training modules
RETRAINING_CURRICULUM = {
    "fire": {
        "hazard_identification": [
            TrainingModule(
                module_id="fire_hazard_001",
                name="Fire Hazard Recognition",
                description="Learn to identify fire sources, fuel, and ignition triggers",
                estimated_duration_minutes=15,
                difficulty_level="beginner",
                competencies_addressed=["hazard_identification"],
                reason=""
            ),
            TrainingModule(
                module_id="fire_hazard_002",
                name="Advanced Hazard Assessment",
                description="Assess hazard severity and decision-making in complex scenarios",
                estimated_duration_minutes=20,
                difficulty_level="advanced",
                competencies_addressed=["hazard_identification", "decision_making"],
                reason=""
            ),
        ],
        "ppe_selection": [
            TrainingModule(
                module_id="fire_ppe_001",
                name="Fire PPE Fundamentals",
                description="Proper selection and donning of fire-resistant clothing and equipment",
                estimated_duration_minutes=12,
                difficulty_level="beginner",
                competencies_addressed=["ppe_selection"],
                reason=""
            ),
            TrainingModule(
                module_id="fire_ppe_002",
                name="Respiratory Protection",
                description="Use and maintenance of SCBA and other respiratory devices",
                estimated_duration_minutes=15,
                difficulty_level="intermediate",
                competencies_addressed=["ppe_selection"],
                reason=""
            ),
        ],
        "procedure_compliance": [
            TrainingModule(
                module_id="fire_proc_001",
                name="Fire Evacuation Procedures",
                description="Step-by-step evacuation protocol and safe exit procedures",
                estimated_duration_minutes=10,
                difficulty_level="beginner",
                competencies_addressed=["procedure_compliance"],
                reason=""
            ),
            TrainingModule(
                module_id="fire_proc_002",
                name="Emergency Communication",
                description="How to alert others and coordinate during emergencies",
                estimated_duration_minutes=8,
                difficulty_level="beginner",
                competencies_addressed=["procedure_compliance", "decision_making"],
                reason=""
            ),
        ],
        "equipment_use": [
            TrainingModule(
                module_id="fire_equip_001",
                name="Fire Extinguisher Basics",
                description="Types, selection, and proper use of fire extinguishers",
                estimated_duration_minutes=15,
                difficulty_level="beginner",
                competencies_addressed=["equipment_use"],
                reason=""
            ),
            TrainingModule(
                module_id="fire_equip_002",
                name="Advanced Fire Equipment",
                description="Sprinklers, suppression systems, and specialized equipment",
                estimated_duration_minutes=20,
                difficulty_level="advanced",
                competencies_addressed=["equipment_use"],
                reason=""
            ),
        ],
        "decision_making": [
            TrainingModule(
                module_id="fire_decision_001",
                name="Threat Assessment",
                description="Quickly evaluate threats and make sound safety decisions",
                estimated_duration_minutes=18,
                difficulty_level="intermediate",
                competencies_addressed=["decision_making"],
                reason=""
            ),
            TrainingModule(
                module_id="fire_decision_002",
                name="Escape vs. Fight Decision",
                description="When to evacuate vs. when to attempt to control a fire",
                estimated_duration_minutes=15,
                difficulty_level="advanced",
                competencies_addressed=["decision_making", "procedure_compliance"],
                reason=""
            ),
        ],
    },
    "gas": {
        "hazard_identification": [
            TrainingModule(
                module_id="gas_hazard_001",
                name="Gas Hazard Recognition",
                description="Detect and identify various toxic gases and their signs",
                estimated_duration_minutes=15,
                difficulty_level="beginner",
                competencies_addressed=["hazard_identification"],
                reason=""
            ),
            TrainingModule(
                module_id="gas_hazard_002",
                name="Gas Concentration Assessment",
                description="Understand concentration levels and health impacts",
                estimated_duration_minutes=12,
                difficulty_level="intermediate",
                competencies_addressed=["hazard_identification"],
                reason=""
            ),
        ],
        "ppe_selection": [
            TrainingModule(
                module_id="gas_ppe_001",
                name="Respirator Selection",
                description="Choose appropriate respirator type for different gases",
                estimated_duration_minutes=15,
                difficulty_level="beginner",
                competencies_addressed=["ppe_selection"],
                reason=""
            ),
            TrainingModule(
                module_id="gas_ppe_002",
                name="Fit Testing and Donning",
                description="Proper respirator fit-testing and correct donning procedure",
                estimated_duration_minutes=20,
                difficulty_level="intermediate",
                competencies_addressed=["ppe_selection"],
                reason=""
            ),
        ],
        "evacuation": [
            TrainingModule(
                module_id="gas_evac_001",
                name="Gas Evacuation Procedures",
                description="Safe evacuation from gas-contaminated areas",
                estimated_duration_minutes=12,
                difficulty_level="beginner",
                competencies_addressed=["evacuation"],
                reason=""
            ),
            TrainingModule(
                module_id="gas_evac_002",
                name="Upwind Movement",
                description="Navigate away from gas source and find fresh air",
                estimated_duration_minutes=10,
                difficulty_level="beginner",
                competencies_addressed=["evacuation"],
                reason=""
            ),
        ],
        "equipment_use": [
            TrainingModule(
                module_id="gas_equip_001",
                name="Gas Detection Equipment",
                description="Operation and maintenance of gas detection equipment",
                estimated_duration_minutes=15,
                difficulty_level="beginner",
                competencies_addressed=["equipment_use"],
                reason=""
            ),
            TrainingModule(
                module_id="gas_equip_002",
                name="Ventilation Systems",
                description="Setup and operation of ventilation and extraction equipment",
                estimated_duration_minutes=18,
                difficulty_level="intermediate",
                competencies_addressed=["equipment_use"],
                reason=""
            ),
        ],
        "emergency_response": [
            TrainingModule(
                module_id="gas_emerg_001",
                name="Gas Emergency Procedures",
                description="Immediate actions when gas incident occurs",
                estimated_duration_minutes=12,
                difficulty_level="beginner",
                competencies_addressed=["emergency_response"],
                reason=""
            ),
            TrainingModule(
                module_id="gas_emerg_002",
                name="Rescue Coordination",
                description="Assist affected personnel and coordinate rescue",
                estimated_duration_minutes=20,
                difficulty_level="advanced",
                competencies_addressed=["emergency_response"],
                reason=""
            ),
        ],
    }
}


class RetrainingRecommender:
    """Generates targeted retraining recommendations based on weaknesses."""
    
    def __init__(self, scenario_type: str = "fire"):
        """
        Initialize recommender for a scenario type.
        
        Args:
            scenario_type: "fire" or "gas"
        """
        self.scenario_type = scenario_type.lower()
        self.curriculum = RETRAINING_CURRICULUM.get(
            self.scenario_type,
            RETRAINING_CURRICULUM["fire"]  # Default to fire
        )
    
    def recommend_retraining(
        self,
        weaknesses: List[Weakness],
        max_modules: int = 3
    ) -> List[TrainingModule]:
        """
        Generate retraining recommendations for detected weaknesses.
        
        Prioritizes:
        1. Severe weaknesses
        2. Core safety competencies
        3. Minimal unnecessary retraining
        
        Args:
            weaknesses: List of Weakness objects from detector
            max_modules: Maximum number of modules to recommend
            
        Returns:
            List of TrainingModule recommendations
        """
        if not weaknesses:
            return []
        
        recommended_modules = []
        core_competencies = {
            "hazard_identification", "ppe_selection", "procedure_compliance"
        }
        
        # Process weaknesses in order of severity
        for weakness in weaknesses:
            if len(recommended_modules) >= max_modules:
                break
            
            competency = weakness.competency_name
            
            # Get modules for this competency
            modules = self.curriculum.get(competency, [])
            if not modules:
                continue
            
            # Choose beginner module for severe weakness, intermediate for moderate
            if weakness.severity == "severe":
                module = next(
                    (m for m in modules if m.difficulty_level == "beginner"),
                    modules[0] if modules else None
                )
            elif weakness.severity == "moderate":
                module = next(
                    (m for m in modules if m.difficulty_level == "intermediate"),
                    modules[0] if modules else None
                )
            else:
                module = modules[0] if modules else None
            
            if module:
                # Create new instance with reason filled in
                rec_module = TrainingModule(
                    module_id=module.module_id,
                    name=module.name,
                    description=module.description,
                    estimated_duration_minutes=module.estimated_duration_minutes,
                    difficulty_level=module.difficulty_level,
                    competencies_addressed=module.competencies_addressed,
                    reason=f"Weakness in {competency}: score {weakness.score:.1f} (severity: {weakness.severity})"
                )
                recommended_modules.append(rec_module)
        
        return recommended_modules
    
    def get_retraining_plan(
        self,
        weaknesses: List[Weakness],
        total_time_limit_minutes: int = 120
    ) -> Dict:
        """
        Generate a complete retraining plan with time estimates.
        
        Args:
            weaknesses: List of detected weaknesses
            total_time_limit_minutes: Maximum recommended training time
            
        Returns:
            Dict with plan structure and metadata
        """
        modules = self.recommend_retraining(weaknesses)
        
        total_time = sum(m.estimated_duration_minutes for m in modules)
        
        return {
            "scenario_type": self.scenario_type,
            "recommended_modules": [
                {
                    "module_id": m.module_id,
                    "name": m.name,
                    "description": m.description,
                    "estimated_duration_minutes": m.estimated_duration_minutes,
                    "difficulty_level": m.difficulty_level,
                    "competencies_addressed": m.competencies_addressed,
                    "reason": m.reason,
                }
                for m in modules
            ],
            "total_estimated_duration_minutes": total_time,
            "time_limit_exceeded": total_time > total_time_limit_minutes,
            "weaknesses_addressed": len(modules),
            "total_weaknesses": len(weaknesses),
        }
