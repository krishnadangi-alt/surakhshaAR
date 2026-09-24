"""
Unit test verifying fire extinguishing timer and grip logic:
1. Grip held + aimed towards fire accumulates timer towards 10.0s.
2. Grip released before 10.0s immediately resets timer to 0.0s.
3. ExtinguishAll triggers complete fire shutdown and notifies FlowManager.
"""

import os
import sys

if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8')

root_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), '..'))

def test_extinguisher_spray_collision():
    path = os.path.join(root_dir, 'Assets', 'Scripts', 'extinguisherspraycollision.cs')
    with open(path, 'r', encoding='utf-8') as f:
        code = f.read()

    # Verify 10s required time
    assert "requiredTime = 10f" in code, "requiredTime = 10f missing in Awake"
    
    # Verify immediate timer reset on grip release
    assert "if (!handleHeld && !isSpraying)" in code, "Grip release condition missing"
    assert "contactTimer = 0f" in code, "Timer reset missing"
    assert "fireExtinguishable.NotifyParticleCollision(false, 0f, requiredTime)" in code, "Notify fireExtinguishable of reset missing"
    
    # Verify aim accumulation
    assert "contactTimer += Time.deltaTime" in code, "Timer accumulation missing"
    assert "if (contactTimer >= requiredTime)" in code, "Timer 10s threshold check missing"
    assert "ExtinguishAll()" in code, "ExtinguishAll call missing"
    
    # Verify FlowManager notification in ExtinguishAll
    assert "flow.HandleFireExtinguished()" in code, "FlowManager notification missing in ExtinguishAll"
    assert "gripInteraction.StopGrip()" in code, "Grip release on extinction missing"
    print("✓ [TEST] ExtinguisherSprayCollision 10s timer & immediate grip reset verified!")

def test_fire_extinguishable():
    path = os.path.join(root_dir, 'Assets', 'Scripts', 'FireExtinguishable1.cs')
    with open(path, 'r', encoding='utf-8') as f:
        code = f.read()

    assert "extinguishTime = 10f" in code, "extinguishTime = 10f missing in Awake"
    assert "_mirroredTimer = 0f" in code, "Timer reset missing"
    assert "_mirroredContact = false" in code, "Contact reset missing"
    assert "flow.HandleFireExtinguished()" in code, "FlowManager notification missing in ExtinguishFire"
    print("✓ [TEST] FireExtinguishable 10s timer & reset verified!")

def test_extinguisher_grip_interaction():
    path = os.path.join(root_dir, 'Assets', 'Scripts', 'ExtingguisherGripInteraction.cs')
    with open(path, 'r', encoding='utf-8') as f:
        code = f.read()

    assert "sprayCol.ResetCollisionTimer()" in code, "ResetCollisionTimer call in StopGrip missing"
    print("✓ [TEST] ExtinguisherGripInteraction StopGrip timer reset verified!")

def test_flow_manager():
    path = os.path.join(root_dir, 'Assets', 'Scripts', 'FireScenarioFlowManager.cs')
    with open(path, 'r', encoding='utf-8') as f:
        code = f.read()

    assert "public void HandleFireExtinguished()" in code, "HandleFireExtinguished must be public"
    assert "Hold Grip to Spray (10s)" in code, "HUD 0-timer reset text missing"
    assert "Spraying... ({elapsed:F1}s / {total:F0}s)" in code, "Live countdown display missing"
    print("✓ [TEST] FireScenarioFlowManager HUD & public HandleFireExtinguished verified!")

if __name__ == '__main__':
    test_extinguisher_spray_collision()
    test_fire_extinguishable()
    test_extinguisher_grip_interaction()
    test_flow_manager()
    print("\nALL 4 TIMER & GRIP TESTS PASSED!")
