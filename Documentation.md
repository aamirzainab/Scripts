# QRManager Documentation

## Overview
The `QRManager` class is our main function file, detecting the `trackedImage`, instantiating the plane, and calling function classes from `UDPSender.cs`.

## Fields
- **`arCamera`**: The camera used for AR.
- **`_trackedImageManager`**: Manages the detection and tracking of images in AR.
- **`qrCodePrefab`**: Prefab instantiated at the location of detected QR codes.
- **`cubePrefab`**, **`planePrefab`**: Prefabs used in the scene.
- **`_occlusionManager`**: Manages occlusion in AR.
- **`_anchorManager`**: Manages AR anchors.
- **`raycastManager`**: Manages raycasting in AR.
- **`_spawnedPrefabs`**: Dictionary to keep track of spawned prefabs associated with tracked images.
- **`_anchors`**: List to keep track of AR anchors.
- **`screen`**: GameObject representing the virtual screen.
- **`screenPosition`**: Vector2 representing the screen position.
- **`interval`**: Time interval for updating screen coordinates.
- **`raycastLinePrefab`**: Prefab for the raycast line.
- **`raycastLine`**: GameObject representing the raycast line.
- **`anchorMarker`**: Prefab for anchor markers.
- **`topLeft`**, **`bottomLeft`**, **`bottomRight`**, **`topRight`**: AR anchors for the screen corners.
- **`myPlane`**: Struct holding data related to the display plane.
- **`lastTapTime`**: Keeps track of the last tap time for double-tap detection.
- **`transformHandle`**: Handle for transforming the screen.
- **`doubleTapDelay`**: Time interval to detect double taps.
- **`normalToPlane`**: Normal vector to the plane.
- **`calibrated`**: Boolean flag indicating if calibration is complete.

## Methods

### PlacePlaneFromImage(ARTrackedImage img)

**Purpose:** This function places a virtual plane in an AR environment according to the orientation and position of a detected QR code or similar tracked image. It calculates the plane's corners relative to the tracked image and sets up AR anchors at these corners for stability in the AR space.

**Detailed Steps:**

- **Calculate Normal and Up Directions:** 
  - Calculate the normal to the plane using the rotation of the detected image and Unity's `Vector3.up`. This normal vector is crucial for aligning the plane in 3D space.
  - Calculate the upward direction of the image in the world space by transforming Unity's `Vector3.forward` with the image's rotation. This vector points in the direction the top of the image is facing.

- **Initialize the Top-Left Corner:** 
  - Set the initial position of the plane to the position of the tracked image, treating it as the top-left corner of the plane.

- **Calculate the Right Direction:** 
  - Compute a right direction vector, which is perpendicular to both the normal and the world up direction (`Vector3.up`). This is done using the cross product of the normal and `Vector3.up`.

- **Convert Dimensions from Inches to Meters:** 
  - Convert the plane's dimensions from inches to meters for internal calculations. Convert the width and height by dividing the inch values by the conversion factor from inches to meters.

- **Adjust the Top-Left Corner Position:** 
  - Adjust the top-left corner by moving it slightly left and up using pre-defined adjustments to position the plane precisely relative to the tracked image.

- **Calculate Other Corners:** 
  - Calculate the positions of the other three corners (top-right, bottom-left, bottom-right) using the adjusted top-left corner. This is achieved by adding/subtracting the scaled right and up vectors from the top-left corner's position.

- **Create AR Anchors at Each Corner:** 
  - Create AR anchors at each of the calculated corners. These anchors stabilize the plane's position in the AR environment, ensuring it remains fixed relative to the real world even as the user moves around.

- **Instantiate and Position the Plane:** 
  - If the plane is being instantiated for the first time (i.e., there is no existing plane object), calculate the center position of the plane from the corners. Instantiate the plane prefab at this position with the appropriate rotation and scale to ensure correct orientation and size in the AR view.

- **Initialize the Gizmo:** 
  - If a gizmo (a graphical interface tool for manipulating the plane's position and orientation) is associated with the plane, initialize it to allow users to interactively adjust the plane's position and orientation during runtime.


# Function: DetermineScreenCoordinates()

## What It Does

Calculates the coordinates on the virtual plane where the camera's center view intersects. This method is crucial for mapping touch inputs or view centers to specific points on the AR plane.

## Steps Explained

**Reset AR World Origin:** 
- Calls `SetARWorldOrigin` to ensure the AR scene's origin is aligned properly, resetting any drifts or misalignments.

**Create and Position Ray:** 
- Casts a ray from the center of the AR camera's viewport (using `new Vector2(0.5f, 0.5f)` for the center point), which aims directly forward from the camera.

**Instantiate Ray Visualization:** 
- If `raycastLine` isn't already set up, it instantiates `raycastLinePrefab` at the AR camera's position for visualizing the ray in the scene.

**Configure Line Renderer:** 
- Sets up the line renderer to show the path of the ray, helping with debugging and visual feedback.

**Perform Raycast Against Plane:** 
- Uses `myPlane._plane.Raycast` to check if and where the ray intersects with the plane defined in `myPlane`.

**Calculate Intersection Details:** 
- If there's an intersection, computes the point (`intersectionPoint`) and normalizes the direction of the ray for accurate measurement.
- The relative position to the top-left anchor (`topLeft`) is calculated to determine the exact location on the plane in plane-relative coordinates.

**Project onto Plane Axes:** 
- Projects the intersection point onto the plane's right and downward axes to convert the 3D point into two 2D coordinates (`xProjected`, `yProjected`).

**Adjust Coordinates:** 
- Adjusts projected coordinates to ensure they are within the plane's bounds, flipping signs if necessary depending on the dot product results.

**Normalize and Adjust Projections:** 
- Converts absolute distances to proportions of the plane's width and height to get normalized screen coordinates (`normalizedX`, `normalizedY`).

**Check Validity and Set Position:** 
- Ensures the calculated screen coordinates are valid (i.e., within `[0,1]` range for both x and y).
- If valid, updates `screenPosition` and sets the positions in the line renderer to visually represent the ray and intersection.

**Send Data (Optional):** 
- Optionally sends screen rotation and coordinate data via a `UdpSender` component if present, useful for applications needing to transmit interaction data over a network.
