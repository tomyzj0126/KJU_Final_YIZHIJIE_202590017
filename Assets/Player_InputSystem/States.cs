public enum TargetPlatform { PC, Quest }

// Main
public enum PlayerState { Movement, Combat, Vehicle, Inventory, Photography, Dialogue}
public enum PlayerInteractionState { Idle, Grabing, Looting, Reading, Mining }
// Subs
public enum PlayerMovementState { Ground, Air, Climb, Swim, Glide, Teleport }
public enum PlayerVehicleState { Mounting, Riding, Riding_Passive }
public enum PlayerCombatState { CombatMove, Cover_Idle, Cover_Aim }

public enum EnemyState { Idle, Patrol, Attack }
