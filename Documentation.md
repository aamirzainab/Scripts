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
  - Initialize gizmo to allow users to interactively adjust the plane's position and orientation during runtime.


### DetermineScreenCoordinates()

**Purpose:** Calculates the coordinates on the virtual plane where the camera's center view intersects. This method is crucial for mapping touch inputs or view centers to specific points on the AR plane.

**Detailed Steps:**

- **Reset AR World Origin:** 
  - Calls `SetARWorldOrigin` to ensure the AR scene's origin is aligned properly, resetting any drifts or misalignments.

- **Create and Position Ray:** 
  - Casts a ray from the center of the AR camera's viewport (using `new Vector2(0.5f, 0.5f)` for the center point), which aims directly forward from the camera.

- **Instantiate Ray Visualization:** 
  - If `raycastLine` isn't already set up, it instantiates `raycastLinePrefab` at the AR camera's position for visualizing the ray in the scene.

- **Configure Line Renderer:** 
  - Sets up the line renderer to show the path of the ray, helping with debugging and visual feedback.

- **Perform Raycast Against Plane:** 
  - Uses `myPlane._plane.Raycast` to check if and where the ray intersects with the plane defined in `myPlane`.

- **Calculate Intersection Point:** 
  - `intersectionPoint = ray.GetPoint(enter);`
  - This line calculates the exact point where the ray intersects the plane. The `enter` variable holds the distance along the ray from its origin to the point of intersection on the plane.

- **Determine Ray Direction:** 
  - `rayDirection = (intersectionPoint - ray.origin).normalized;`
  - This determines the direction of the ray at the point of intersection, normalized to ensure it has a unit length. Normalization helps maintain accuracy in subsequent calculations involving angles and projections.

- **Calculate Intersection Details:** 
  - If there's an intersection, computes the point (`intersectionPoint`) and normalizes the direction of the ray for accurate measurement.
  - The relative position to the top-left anchor (`topLeft`) is calculated to determine the exact location on the plane in plane-relative coordinates.

- **Calculate Relative Position:** 
  - `relativePosition = intersectionPoint - topLeft.transform.position;`
  - This finds the vector from the top-left corner of the plane (defined by the anchor at topLeft) to the intersection point. It's used to determine how far along and down the plane the intersection occurs.

- **Define Right and Down Vectors:** 
  - `rightVector = (topRight.transform.position - topLeft.transform.position).normalized;`
  - `downVector = (bottomLeft.transform.position - topLeft.transform.position).normalized;`
  - Definining  the right and down normalized vectors of the plane.  

- **Project Relative Position:** 
  - `xProjected = Vector3.Project(relativePosition, rightVector).magnitude;`
  - `yProjected = Vector3.Project(relativePosition, downVector).magnitude;`
  - Here, the relative position vector is projected onto the plane’s right and down vectors. This step converts the 3D intersection coordinates into two 2D coordinates relative to the plane’s local axes.

- **Adjust Coordinates:** 
  - The dot products `Vector3.Dot(relativePosition, rightVector)` and `Vector3.Dot(relativePosition, downVector)` are checked to determine if the projected points fall on the negative side of the top-left origin. If so, the projections are negated to accurately reflect their positions relative to the top-left corner.

- **Normalize Coordinates:** 
  - `normalizedX = xProjected / Vector3.Distance(topLeft.transform.position, topRight.transform.position);`
  - `normalizedY = yProjected / Vector3.Distance(topLeft.transform.position, bottomLeft.transform.position);`
  - The projected distances are normalized by dividing by the actual distances between the relevant corners of the plane. This converts the measurements into a scale from 0 to 1, where 0 represents the top/left edge, and 1 represents the bottom/right edge of the plane.


- **Send Data:** 
  - Send iPad 6dof and screen-coordinate data via a `UdpSender` component
