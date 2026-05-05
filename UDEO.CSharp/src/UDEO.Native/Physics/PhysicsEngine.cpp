#include "PhysicsEngine.h"
#include <cmath>
#include <algorithm>

#define UDEO_NATIVE_EXPORTS

// ============================================================
// AABB Collision Detection
// ============================================================
UDEO_NATIVE_API UdeoCollisionResult udeo_aabb_collision(UdeoAABB a, UdeoAABB b) {
    UdeoCollisionResult result = {0, 0.0, 0.0, 0.0, 0.0, 0.0};

    double aRight = a.left + a.width;
    double aBottom = a.top + a.height;
    double bRight = b.left + b.width;
    double bBottom = b.top + b.height;

    if (a.left >= bRight || aRight <= b.left || a.top >= bBottom || aBottom <= b.top) {
        return result; // No collision
    }

    result.collided = 1;

    // Calculate penetration on each axis
    double overlapLeft = aRight - b.left;
    double overlapRight = bRight - a.left;
    double overlapTop = aBottom - b.top;
    double overlapBottom = bBottom - a.top;

    // Find minimum penetration axis
    double minPenetrationX = std::min(overlapLeft, overlapRight);
    double minPenetrationY = std::min(overlapTop, overlapBottom);

    if (minPenetrationX < minPenetrationY) {
        result.penetration = minPenetrationX;
        if (overlapLeft < overlapRight) {
            result.normalX = -1.0;
            result.resolvedX = a.left - minPenetrationX;
        } else {
            result.normalX = 1.0;
            result.resolvedX = a.left + minPenetrationX;
        }
        result.normalY = 0.0;
        result.resolvedY = a.top;
    } else {
        result.penetration = minPenetrationY;
        if (overlapTop < overlapBottom) {
            result.normalY = -1.0;
            result.resolvedY = a.top - minPenetrationY;
        } else {
            result.normalY = 1.0;
            result.resolvedY = a.top + minPenetrationY;
        }
        result.normalX = 0.0;
        result.resolvedX = a.left;
    }

    return result;
}

// ============================================================
// Physics: Gravity with terminal velocity
// ============================================================
UDEO_NATIVE_API double udeo_apply_gravity(double vy, double gravity, double maxFallSpeed, double dt) {
    double newVy = vy + gravity * dt;
    if (newVy > maxFallSpeed) {
        newVy = maxFallSpeed;
    }
    return newVy;
}

// ============================================================
// 1D Axis Collision Resolution
// ============================================================
UDEO_NATIVE_API double udeo_resolve_axis_collision(
    double position, double velocity, double minBound, double maxBound) {
    if (position < minBound) return minBound;
    if (position > maxBound) return maxBound;
    return position;
}

// ============================================================
// Grid-based Collision Check (hot path for game rendering)
// ============================================================
UDEO_NATIVE_API int udeo_grid_collision(
    const unsigned char* grid, int gridWidth, int gridHeight,
    int checkLeft, int checkTop, int checkWidth, int checkHeight) {

    int checkRight = checkLeft + checkWidth;
    int checkBottom = checkTop + checkHeight;

    // Clamp to grid bounds
    if (checkLeft < 0) checkLeft = 0;
    if (checkTop < 0) checkTop = 0;
    if (checkRight > gridWidth) checkRight = gridWidth;
    if (checkBottom > gridHeight) checkBottom = gridHeight;

    for (int row = checkTop; row < checkBottom; ++row) {
        for (int col = checkLeft; col < checkRight; ++col) {
            if (grid[row * gridWidth + col] != 0) {
                return 1; // Solid cell found
            }
        }
    }
    return 0;
}

// ============================================================
// Grid Raycast (DDA algorithm)
// ============================================================
UDEO_NATIVE_API UdeoCollisionResult udeo_grid_raycast(
    const unsigned char* grid, int gridWidth, int gridHeight,
    double startX, double startY, double dirX, double dirY, double maxDistance) {

    UdeoCollisionResult result = {0, 0.0, 0.0, 0.0, 0.0, 0.0};

    // DDA (Digital Differential Analyzer) raycast
    int mapX = (int)std::floor(startX);
    int mapY = (int)std::floor(startY);

    double deltaDistX = (dirX == 0) ? 1e30 : std::abs(1.0 / dirX);
    double deltaDistY = (dirY == 0) ? 1e30 : std::abs(1.0 / dirY);

    int stepX, stepY;
    double sideDistX, sideDistY;

    if (dirX < 0) {
        stepX = -1;
        sideDistX = (startX - mapX) * deltaDistX;
    } else {
        stepX = 1;
        sideDistX = (mapX + 1.0 - startX) * deltaDistX;
    }

    if (dirY < 0) {
        stepY = -1;
        sideDistY = (startY - mapY) * deltaDistY;
    } else {
        stepY = 1;
        sideDistY = (mapY + 1.0 - startY) * deltaDistY;
    }

    double distance = 0.0;
    int maxSteps = 100;

    while (maxSteps-- > 0) {
        if (mapX < 0 || mapX >= gridWidth || mapY < 0 || mapY >= gridHeight) break;

        if (grid[mapY * gridWidth + mapX] != 0) {
            result.collided = 1;
            result.penetration = distance;
            result.resolvedX = startX + dirX * distance;
            result.resolvedY = startY + dirY * distance;

            // Determine normal based on which side was hit
            if (sideDistX < sideDistY) {
                result.normalX = static_cast<double>(-stepX);
                result.normalY = 0.0;
            } else {
                result.normalX = 0.0;
                result.normalY = static_cast<double>(-stepY);
            }
            return result;
        }

        if (sideDistX < sideDistY) {
            distance = sideDistX;
            sideDistX += deltaDistX;
            mapX += stepX;
        } else {
            distance = sideDistY;
            sideDistY += deltaDistY;
            mapY += stepY;
        }

        if (distance > maxDistance) break;
    }

    return result;
}

// ============================================================
// Financial Math (C++ optimized)
// ============================================================
UDEO_NATIVE_API double udeo_mortgage_payment(
    double principal, double annualRate, int months) {

    if (months <= 0 || principal <= 0.0) return 0.0;

    double monthlyRate = annualRate / 12.0;

    if (std::abs(monthlyRate) < 1e-12) {
        return principal / months;
    }

    double factor = std::pow(1.0 + monthlyRate, months);
    return principal * (monthlyRate * factor) / (factor - 1.0);
}

UDEO_NATIVE_API double udeo_dti_ratio(
    double monthlyDebt, double monthlyIncome) {

    if (monthlyIncome <= 0.0) return 0.0;
    return (monthlyDebt / monthlyIncome) * 100.0;
}

UDEO_NATIVE_API double udeo_ltv_ratio(
    double loanAmount, double propertyValue) {

    if (propertyValue <= 0.0) return 0.0;
    return (loanAmount / propertyValue) * 100.0;
}
