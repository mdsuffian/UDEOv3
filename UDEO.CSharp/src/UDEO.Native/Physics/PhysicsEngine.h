#pragma once

#ifdef UDEO_NATIVE_EXPORTS
#define UDEO_NATIVE_API __declspec(dllexport)
#else
#define UDEO_NATIVE_API __declspec(dllimport)
#endif

// UDEO Native Physics Engine
// High-performance collision detection and physics for expert pipeline games.

#ifdef __cplusplus
extern "C" {
#endif

/// <summary>
/// Vector2D with double precision for physics calculations.
/// </summary>
typedef struct {
    double x;
    double y;
} UdeoVec2D;

/// <summary>
/// Axis-aligned bounding box.
/// </summary>
typedef struct {
    double left;
    double top;
    double width;
    double height;
} UdeoAABB;

/// <summary>
/// Collision result with contact normal and penetration depth.
/// </summary>
typedef struct {
    int collided;           // 1 if collision detected, 0 otherwise
    double normalX;         // Contact normal X
    double normalY;         // Contact normal Y
    double penetration;     // Penetration depth
    double resolvedX;       // Resolved position X
    double resolvedY;       // Resolved position Y
} UdeoCollisionResult;

/// <summary>
/// Tests AABB vs AABB collision.
/// </summary>
UDEO_NATIVE_API UdeoCollisionResult udeo_aabb_collision(UdeoAABB a, UdeoAABB b);

/// <summary>
/// Applies gravity to a vertical velocity with terminal velocity clamping.
/// </summary>
UDEO_NATIVE_API double udeo_apply_gravity(
    double vy,
    double gravity,
    double maxFallSpeed,
    double dt);

/// <summary>
/// Resolves a 1D axis collision, returning the corrected position.
/// </summary>
UDEO_NATIVE_API double udeo_resolve_axis_collision(
    double position,
    double velocity,
    double minBound,
    double maxBound);

/// <summary>
/// Performs a grid-based collision check for a 2D grid of cells.
/// Returns 1 if any cell at the given position is solid.
/// Grid is a flat array of bytes, 0 = air, non-0 = solid.
/// </summary>
UDEO_NATIVE_API int udeo_grid_collision(
    const unsigned char* grid,
    int gridWidth,
    int gridHeight,
    int checkLeft,
    int checkTop,
    int checkWidth,
    int checkHeight);

/// <summary>
/// Raycast through a grid, returns hit distance and cell coordinates.
/// </summary>
UDEO_NATIVE_API UdeoCollisionResult udeo_grid_raycast(
    const unsigned char* grid,
    int gridWidth,
    int gridHeight,
    double startX,
    double startY,
    double dirX,
    double dirY,
    double maxDistance);

/// <summary>
/// Computes mortgage monthly payment (standard amortization formula).
/// </summary>
UDEO_NATIVE_API double udeo_mortgage_payment(
    double principal,
    double annualRate,
    int months);

/// <summary>
/// Computes debt-to-income ratio as a percentage.
/// </summary>
UDEO_NATIVE_API double udeo_dti_ratio(
    double monthlyDebt,
    double monthlyIncome);

/// <summary>
/// Computes loan-to-value ratio as a percentage.
/// </summary>
UDEO_NATIVE_API double udeo_ltv_ratio(
    double loanAmount,
    double propertyValue);

#ifdef __cplusplus
}
#endif
