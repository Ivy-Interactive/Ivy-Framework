using Ivy.Shared;


namespace Ivy.Samples.Shared.Apps.Workshops;

[App(icon: Icons.Box, path: ["Workshops"], isVisible: true, title: "Three.js Integration Workshop", searchHints: ["3d", "three.js", "webgl", "graphics", "rendering", "workshop", "requirements", "visualization"])]
public class ThreeJsWorkshopApp : SampleBase
{
    protected override object? BuildSample()
    {
        return new Article()
            | new Markdown(
                """"
                # Project Idea: Three.js Integration for Ivy Framework (January 2025)

                We want to bring Three.js functionality into Ivy-Framework, enabling .NET developers to render 3D graphics and interactive 3D scenes directly from their C# applications.
                """"
            )
            | new Callout("This is a requirements sketch; it does not implement the feature.", icon: Icons.Info)
            | new Markdown(
                """"
                ## Goals

                * **3D Scene Rendering**: Enable developers to create and render 3D scenes using declarative widgets in C#.
                * **Scene Graph Composition**: Support hierarchical scene composition with meshes, lights, cameras, and other 3D objects.
                * **Interactive 3D Content**: Enable user interactions (clicks, drags, rotations) with 3D objects that trigger C# event handlers.
                * **Material and Texture Support**: Support various materials, textures, and lighting models.
                * **Animation Support**: Enable smooth animations and transitions for 3D objects.
                * **Performance Optimization**: Lazy-load Three.js library only when 3D content is present.
                * **Theme Integration**: Automatic dark/light theme support for 3D scenes.
                * **Responsive Design**: 3D scenes adapt to container size and maintain aspect ratios.
                """"
            )
            | new Markdown(
                """"
                ## Non-Goals (initial)

                * Advanced physics simulation (can be future work).
                * VR/AR support (can be future work).
                * Server-side 3D rendering or ray tracing.
                * Custom shader authoring (use Three.js built-in materials initially).
                * 3D model import from external formats (GLTF/OBJ support can be added later).
                * Real-time multiplayer 3D experiences.
                """"
            )
            | new Markdown(
                """"
                ## User Stories

                1. **As a developer**, I can create a 3D scene widget in C# and see it rendered in the browser.
                2. **As a developer**, I can add meshes (cubes, spheres, planes) to my scene with materials and colors.
                3. **As a developer**, I can position and rotate 3D objects using C# properties.
                4. **As a developer**, I can add lights (ambient, directional, point) to illuminate my scene.
                5. **As a developer**, I can configure camera position, rotation, and projection type.
                6. **As a user**, I can interact with 3D objects (click, hover, drag) and trigger C# event handlers.
                7. **As a developer**, I can animate 3D objects using C# state management.
                8. **As a developer**, I can compose complex 3D scenes by nesting scene graph elements.
                9. **As a user**, I see 3D scenes that respect my theme preferences (dark/light mode).
                10. **As a developer**, I can control rendering performance with quality settings.
                """"
            )
            | new Markdown(
                """"
                ## Proposed Architecture

                ### Backend (C#) - Widget Definitions

                The backend will provide declarative widgets that mirror Three.js concepts:

                ```csharp
                // Core scene widget
                public record Scene3D : WidgetBase<Scene3D>
                {
                    [Prop] public Camera3D? Camera { get; set; }
                    [Prop] public Light3D[]? Lights { get; set; }
                    [Prop] public Scene3DOptions? Options { get; set; }
                }

                // Camera widget
                public record Camera3D : WidgetBase<Camera3D>
                {
                    [Prop] public CameraType Type { get; set; } = CameraType.Perspective;
                    [Prop] public Vector3 Position { get; set; } = new(0, 0, 5);
                    [Prop] public Vector3? Target { get; set; }
                    [Prop] public float? Fov { get; set; }
                    [Prop] public float? Near { get; set; }
                    [Prop] public float? Far { get; set; }
                }

                // Mesh widget
                public record Mesh3D : WidgetBase<Mesh3D>
                {
                    [Prop] public Geometry3D Geometry { get; set; }
                    [Prop] public Material3D Material { get; set; }
                    [Prop] public Vector3 Position { get; set; } = Vector3.Zero;
                    [Prop] public Vector3 Rotation { get; set; } = Vector3.Zero;
                    [Prop] public Vector3 Scale { get; set; } = Vector3.One;
                    [Event] public Event<Vector3>? OnClick { get; set; }
                    [Event] public Event<Vector3>? OnHover { get; set; }
                }

                // Geometry types
                public record Geometry3D
                {
                    public static Geometry3D Box(float width = 1, float height = 1, float depth = 1) => 
                        new() { Type = GeometryType.Box, Width = width, Height = height, Depth = depth };
                    public static Geometry3D Sphere(float radius = 1, int segments = 32) => 
                        new() { Type = GeometryType.Sphere, Radius = radius, Segments = segments };
                    public static Geometry3D Plane(float width = 1, float height = 1) => 
                        new() { Type = GeometryType.Plane, Width = width, Height = height };
                    // ... more geometry types
                }

                // Material types
                public record Material3D
                {
                    public static Material3D MeshBasic(Color? color = null) => 
                        new() { Type = MaterialType.MeshBasic, Color = color ?? Color.White };
                    public static Material3D MeshStandard(Color? color = null, float? roughness = null, float? metalness = null) => 
                        new() { Type = MaterialType.MeshStandard, Color = color ?? Color.White, Roughness = roughness, Metalness = metalness };
                    // ... more material types
                }

                // Light widgets
                public record AmbientLight3D : WidgetBase<AmbientLight3D>
                {
                    [Prop] public Color Color { get; set; } = Color.White;
                    [Prop] public float Intensity { get; set; } = 1.0f;
                }

                public record DirectionalLight3D : WidgetBase<DirectionalLight3D>
                {
                    [Prop] public Color Color { get; set; } = Color.White;
                    [Prop] public float Intensity { get; set; } = 1.0f;
                    [Prop] public Vector3 Position { get; set; } = new(0, 1, 0);
                }

                public record PointLight3D : WidgetBase<PointLight3D>
                {
                    [Prop] public Color Color { get; set; } = Color.White;
                    [Prop] public float Intensity { get; set; } = 1.0f;
                    [Prop] public float Distance { get; set; } = 0;
                    [Prop] public float Decay { get; set; } = 1.0f;
                    [Prop] public Vector3 Position { get; set; } = Vector3.Zero;
                }
                ```

                ### Frontend (React/TypeScript) - Three.js Integration

                The frontend will create React components that use Three.js:

                ```typescript
                // frontend/src/widgets/threed/Scene3DWidget.tsx
                import React, { useEffect, useRef, useState } from 'react';
                import * as THREE from 'three';
                import { useTheme } from '@/components/theme-provider/hooks';

                interface Scene3DWidgetProps {
                  id: string;
                  camera?: Camera3DProps;
                  lights?: Light3DProps[];
                  options?: Scene3DOptions;
                  children?: React.ReactNode;
                }

                const Scene3DWidget: React.FC<Scene3DWidgetProps> = ({
                  camera,
                  lights,
                  options,
                  children,
                }) => {
                  const containerRef = useRef<HTMLDivElement>(null);
                  const sceneRef = useRef<THREE.Scene | null>(null);
                  const rendererRef = useRef<THREE.WebGLRenderer | null>(null);
                  const animationFrameRef = useRef<number>();
                  const { theme } = useTheme();

                  useEffect(() => {
                    if (!containerRef.current) return;

                    // Initialize Three.js scene
                    const scene = new THREE.Scene();
                    scene.background = new THREE.Color(
                      theme === 'dark' ? '#000000' : '#ffffff'
                    );

                    // Setup camera
                    const cam = camera?.type === 'orthographic' 
                      ? new THREE.OrthographicCamera(...)
                      : new THREE.PerspectiveCamera(
                          camera?.fov ?? 75,
                          containerRef.current.clientWidth / containerRef.current.clientHeight,
                          camera?.near ?? 0.1,
                          camera?.far ?? 1000
                        );
                    cam.position.set(
                      camera?.position?.x ?? 0,
                      camera?.position?.y ?? 0,
                      camera?.position?.z ?? 5
                    );

                    // Setup renderer
                    const renderer = new THREE.WebGLRenderer({ 
                      antialias: options?.antialias ?? true 
                    });
                    renderer.setSize(
                      containerRef.current.clientWidth,
                      containerRef.current.clientHeight
                    );
                    renderer.setPixelRatio(window.devicePixelRatio);
                    containerRef.current.appendChild(renderer.domElement);

                    sceneRef.current = scene;
                    rendererRef.current = renderer;

                    // Animation loop
                    const animate = () => {
                      animationFrameRef.current = requestAnimationFrame(animate);
                      renderer.render(scene, cam);
                    };
                    animate();

                    // Cleanup
                    return () => {
                      if (animationFrameRef.current) {
                        cancelAnimationFrame(animationFrameRef.current);
                      }
                      renderer.dispose();
                      containerRef.current?.removeChild(renderer.domElement);
                    };
                  }, [camera, options, theme]);

                  // Handle resize
                  useEffect(() => {
                    const handleResize = () => {
                      if (!containerRef.current || !rendererRef.current || !sceneRef.current) return;
                      // Update camera and renderer size
                    };
                    window.addEventListener('resize', handleResize);
                    return () => window.removeEventListener('resize', handleResize);
                  }, []);

                  return (
                    <div 
                      ref={containerRef} 
                      className="w-full h-full"
                      style={{ minHeight: options?.minHeight ?? '400px' }}
                    />
                  );
                };
                ```
                """"
            )
            | new Markdown(
                """"
                ## Technical Implementation Details

                ### 1. Widget Serialization

                Widgets will serialize to JSON following Ivy's standard pattern:
                - Properties marked with `[Prop]` are serialized to camelCase JSON
                - Events marked with `[Event]` are registered for frontend invocation
                - Child widgets are serialized recursively
                - Type information is preserved via namespace.classname format

                ### 2. Frontend Widget Registration

                Widgets will be registered in `frontend/src/widgets/widgetMap.ts`:

                ```typescript
                import { Scene3DWidget } from '@/widgets/threed/Scene3DWidget';
                import { Mesh3DWidget } from '@/widgets/threed/Mesh3DWidget';
                // ... more widgets

                export const widgetMap = {
                  // ... existing widgets
                  'Ivy.Scene3D': React.lazy(() => import('@/widgets/threed/Scene3DWidget')),
                  'Ivy.Mesh3D': React.lazy(() => import('@/widgets/threed/Mesh3DWidget')),
                  'Ivy.Camera3D': React.lazy(() => import('@/widgets/threed/Camera3DWidget')),
                  'Ivy.AmbientLight3D': React.lazy(() => import('@/widgets/threed/AmbientLight3DWidget')),
                  'Ivy.DirectionalLight3D': React.lazy(() => import('@/widgets/threed/DirectionalLight3DWidget')),
                  'Ivy.PointLight3D': React.lazy(() => import('@/widgets/threed/PointLight3DWidget')),
                };
                ```

                ### 3. Three.js Library Loading

                Three.js will be lazy-loaded only when 3D widgets are present:

                ```typescript
                // frontend/src/widgets/threed/loadThree.ts
                let threeLoaded = false;
                let threeLoadPromise: Promise<void> | null = null;

                export const loadThree = (): Promise<void> => {
                  if (threeLoaded) return Promise.resolve();
                  if (threeLoadPromise) return threeLoadPromise;

                  threeLoadPromise = import('three').then(() => {
                    threeLoaded = true;
                  });

                  return threeLoadPromise;
                };
                ```

                Add Three.js to `frontend/package.json`:
                ```json
                {
                  "dependencies": {
                    "three": "^0.170.0"
                  }
                }
                ```

                Update `vite.config.ts` for chunking:
                ```typescript
                manualChunks(id) {
                  if (id.includes('node_modules')) {
                    if (id.includes('three')) return 'vendor-three';
                    // ... existing chunks
                  }
                }
                ```

                ### 4. Event Handling

                Mouse interactions will be captured and sent back to C#:

                ```typescript
                // In Mesh3DWidget.tsx
                const handleClick = (event: MouseEvent, mesh: THREE.Mesh) => {
                  // Calculate world position
                  const worldPosition = new THREE.Vector3();
                  mesh.getWorldPosition(worldPosition);
                  
                  // Invoke C# event handler
                  invokeEvent('onClick', [
                    { x: worldPosition.x, y: worldPosition.y, z: worldPosition.z }
                  ]);
                };
                ```

                ### 5. State Management and Animations

                Animations can be driven by C# state updates:

                ```csharp
                // Example: Rotating cube
                var rotationState = this.UseState<Vector3>(new Vector3(0, 0, 0));
                
                // Update rotation in C# (e.g., via timer or user interaction)
                rotationState.Value = new Vector3(0, rotationState.Value.Y + 0.01f, 0);
                
                return new Scene3D()
                    | new Mesh3D(
                        Geometry3D.Box(),
                        Material3D.MeshStandard(Color.Blue)
                    )
                    .Rotation(rotationState.Value);
                ```

                Frontend will detect prop changes and update Three.js objects accordingly.

                ### 6. Theme Integration

                Scene backgrounds and material colors will adapt to theme:

                ```typescript
                const { theme } = useTheme();
                
                useEffect(() => {
                  if (sceneRef.current) {
                    sceneRef.current.background = new THREE.Color(
                      theme === 'dark' ? '#000000' : '#ffffff'
                    );
                  }
                }, [theme]);
                ```
                """"
            )
            | new Markdown(
                """"
                ## Example Usage

                ### Basic Scene

                ```csharp
                return new Scene3D()
                    | new Camera3D()
                        .Position(new Vector3(0, 0, 5))
                    | new AmbientLight3D()
                        .Intensity(0.5f)
                    | new DirectionalLight3D()
                        .Position(new Vector3(5, 5, 5))
                        .Intensity(1.0f)
                    | new Mesh3D(
                        Geometry3D.Box(1, 1, 1),
                        Material3D.MeshStandard(Color.Blue)
                    )
                    .Position(new Vector3(0, 0, 0));
                ```

                ### Interactive Scene

                ```csharp
                var selectedMesh = this.UseState<Vector3?>(null);
                
                return new Scene3D()
                    | new Camera3D().Position(new Vector3(0, 0, 5))
                    | new AmbientLight3D()
                    | new Mesh3D(
                        Geometry3D.Sphere(1),
                        Material3D.MeshStandard(
                            selectedMesh.Value.HasValue ? Color.Red : Color.Blue
                        )
                    )
                    .OnClick(pos => selectedMesh.Value = pos)
                    | new TextBlock($"Selected: {selectedMesh.Value}");
                ```

                ### Animated Scene

                ```csharp
                var rotation = this.UseState<float>(0f);
                
                // Update rotation (e.g., via timer or user input)
                rotation.Value += 0.01f;
                
                return new Scene3D()
                    | new Camera3D().Position(new Vector3(0, 0, 5))
                    | new AmbientLight3D()
                    | new Mesh3D(
                        Geometry3D.Box(1, 1, 1),
                        Material3D.MeshStandard(Color.Green)
                    )
                    .Rotation(new Vector3(0, rotation.Value, 0));
                ```

                ### Complex Scene with Multiple Objects

                ```csharp
                return new Scene3D()
                    | new Camera3D()
                        .Position(new Vector3(5, 5, 5))
                        .Target(new Vector3(0, 0, 0))
                    | new AmbientLight3D().Intensity(0.3f)
                    | new DirectionalLight3D()
                        .Position(new Vector3(10, 10, 5))
                        .Intensity(1.0f)
                    | new Mesh3D(
                        Geometry3D.Box(2, 2, 2),
                        Material3D.MeshStandard(Color.Red, roughness: 0.5f, metalness: 0.8f)
                    )
                    .Position(new Vector3(-2, 0, 0))
                    | new Mesh3D(
                        Geometry3D.Sphere(1, 32),
                        Material3D.MeshStandard(Color.Blue, roughness: 0.2f)
                    )
                    .Position(new Vector3(2, 0, 0))
                    | new Mesh3D(
                        Geometry3D.Plane(10, 10),
                        Material3D.MeshStandard(Color.Gray, roughness: 1.0f)
                    )
                    .Position(new Vector3(0, -2, 0))
                    .Rotation(new Vector3(-MathF.PI / 2, 0, 0));
                ```
                """"
            )
            | new Markdown(
                """"
                ## File Structure

                ### Backend Files

                ```
                Ivy/
                ├── Widgets/
                │   └── ThreeD/
                │       ├── Scene3D.cs
                │       ├── Camera3D.cs
                │       ├── Mesh3D.cs
                │       ├── Geometry3D.cs
                │       ├── Material3D.cs
                │       ├── Lights/
                │       │   ├── AmbientLight3D.cs
                │       │   ├── DirectionalLight3D.cs
                │       │   └── PointLight3D.cs
                │       └── Helpers/
                │           ├── Vector3.cs
                │           └── Color3D.cs
                ```

                ### Frontend Files

                ```
                frontend/src/widgets/
                └── threed/
                    ├── Scene3DWidget.tsx
                    ├── Camera3DWidget.tsx
                    ├── Mesh3DWidget.tsx
                    ├── AmbientLight3DWidget.tsx
                    ├── DirectionalLight3DWidget.tsx
                    ├── PointLight3DWidget.tsx
                    ├── loadThree.ts
                    ├── types.ts
                    └── utils.ts
                ```
                """"
            )
            | new Markdown(
                """"
                ## Acceptance Criteria

                ### Phase 1: Core Rendering
                - [ ] `Scene3D` widget renders a WebGL canvas
                - [ ] `Camera3D` widget configures camera position and projection
                - [ ] `Mesh3D` widget renders basic geometries (box, sphere, plane)
                - [ ] `Material3D` supports basic materials (MeshBasic, MeshStandard)
                - [ ] Three.js library lazy-loads only when 3D widgets are present
                - [ ] Scenes adapt to container size and maintain aspect ratio

                ### Phase 2: Lighting
                - [ ] `AmbientLight3D` widget provides ambient lighting
                - [ ] `DirectionalLight3D` widget provides directional lighting
                - [ ] `PointLight3D` widget provides point lighting
                - [ ] Multiple lights can be added to a scene
                - [ ] Light properties (color, intensity, position) are configurable

                ### Phase 3: Interactions
                - [ ] `Mesh3D` supports `OnClick` event handler
                - [ ] `Mesh3D` supports `OnHover` event handler
                - [ ] Click events return world position coordinates
                - [ ] Raycasting works correctly for object selection
                - [ ] Event handlers invoke C# methods via SignalR

                ### Phase 4: Animations
                - [ ] Mesh position, rotation, and scale can be updated from C# state
                - [ ] Smooth transitions when properties change
                - [ ] Animation loop runs efficiently (60 FPS target)
                - [ ] Multiple animated objects perform well

                ### Phase 5: Advanced Features
                - [ ] Additional geometry types (cylinder, torus, etc.)
                - [ ] Additional material types (MeshPhong, MeshLambert, etc.)
                - [ ] Texture support (can be added later)
                - [ ] Theme integration (dark/light mode backgrounds)
                - [ ] Performance optimization (frustum culling, LOD)

                ### Quality Requirements
                - [ ] Error handling: graceful fallback if WebGL is unavailable
                - [ ] Memory management: proper cleanup of Three.js resources
                - [ ] Responsive design: scenes work on mobile devices
                - [ ] Documentation: XML docs for all public APIs
                - [ ] Examples: comprehensive sample applications
                """"
            )
            | new Markdown(
                """"
                ## Risks & Considerations

                ### Technical Risks

                * **WebGL Support**: Not all browsers/devices support WebGL. Need graceful fallback.
                * **Performance**: Complex 3D scenes may impact performance, especially on mobile devices.
                * **Memory Usage**: Three.js objects consume GPU memory. Need proper cleanup.
                * **Bundle Size**: Three.js adds significant bundle size (~600KB). Lazy loading mitigates this.
                * **State Synchronization**: Keeping C# state and Three.js scene in sync can be complex.
                * **Event Handling**: Converting mouse/pointer events to 3D coordinates requires raycasting.

                ### Architecture Risks

                * **Widget Tree Complexity**: Deep scene graphs may impact serialization performance.
                * **Type Safety**: Ensuring type safety between C# and TypeScript representations.
                * **Backward Compatibility**: Adding new 3D widgets shouldn't break existing code.
                * **Testing**: Testing 3D rendering in automated tests is challenging.

                ### UX Risks

                * **Learning Curve**: Developers need to understand 3D concepts (vectors, matrices, etc.).
                * **Mobile Experience**: Touch interactions for 3D scenes may be unintuitive.
                * **Accessibility**: 3D content may not be accessible to all users.
                * **Loading States**: Large 3D scenes may take time to initialize.

                ### Mitigation Strategies

                * **WebGL Detection**: Check for WebGL support and show fallback message.
                * **Performance Monitoring**: Add performance metrics and quality settings.
                * **Resource Management**: Implement proper disposal patterns for Three.js objects.
                * **Progressive Enhancement**: Start with simple scenes, add complexity gradually.
                * **Documentation**: Provide comprehensive guides and examples.
                * **Error Boundaries**: Wrap 3D widgets in error boundaries to prevent crashes.
                """"
            )
            | new Markdown(
                """"
                ## Open Questions

                1. **Geometry API Design**: Should geometries be records with static factory methods, or separate widget types (e.g., `Box3D`, `Sphere3D`)?

                2. **Material API Design**: Similar question for materials - factory methods vs. separate widgets?

                3. **Scene Graph Hierarchy**: How should nested transforms work? Should we support grouping objects?

                4. **Texture Support**: How should textures be provided? URLs, base64, or binary data?

                5. **Animation API**: Should animations be declarative (e.g., `Animate.Rotate(duration, easing)`) or imperative (update state in C#)?

                6. **Performance Settings**: Should we expose quality/performance settings (e.g., antialiasing, shadow quality)?

                7. **GLTF/Model Loading**: Should we support loading external 3D models? If so, how?

                8. **Controls**: Should we provide built-in camera controls (orbit, fly, etc.) or let developers implement their own?

                9. **Shadows**: Should shadow support be included in initial version?

                10. **Post-Processing**: Should we support post-processing effects (bloom, SSAO, etc.)?

                11. **Testing Strategy**: How do we test 3D rendering in automated tests? Mock Three.js?

                12. **Documentation**: Should we provide interactive 3D examples in the docs?
                """"
            )
            | new Markdown(
                """"
                ## Implementation Phases

                ### Phase 1: Proof of Concept (Week 1-2)
                - Create basic `Scene3D` and `Mesh3D` widgets
                - Implement frontend Three.js integration
                - Render a simple rotating cube
                - Verify event handling works

                ### Phase 2: Core Features (Week 3-4)
                - Add camera widget with perspective/orthographic support
                - Implement basic lighting (ambient, directional, point)
                - Add more geometry types (sphere, plane)
                - Add material types (MeshBasic, MeshStandard)

                ### Phase 3: Interactions (Week 5-6)
                - Implement click and hover event handling
                - Add raycasting for object selection
                - Test event propagation to C# handlers
                - Add example interactive scenes

                ### Phase 4: Polish & Documentation (Week 7-8)
                - Add comprehensive examples
                - Write XML documentation
                - Performance optimization
                - Theme integration
                - Error handling and fallbacks

                ### Phase 5: Advanced Features (Future)
                - Additional geometries and materials
                - Texture support
                - Animation helpers
                - GLTF model loading
                - Post-processing effects
                """"
            )
            | new Markdown(
                """"
                ## Related Work

                * **React Three Fiber**: A React renderer for Three.js that inspired some API design ideas.
                * **Babylon.js**: Alternative 3D engine, but Three.js has larger community and better documentation.
                * **A-Frame**: WebVR framework, but too opinionated for our use case.
                * **Existing Ivy Widgets**: Similar patterns used in `DbmlCanvasWidget` (ReactFlow integration).

                ## References

                * [Three.js Documentation](https://threejs.org/docs/)
                * [Three.js Examples](https://threejs.org/examples/)
                * [WebGL Fundamentals](https://webglfundamentals.org/)
                * [React Three Fiber](https://docs.pmnd.rs/react-three-fiber/getting-started/introduction)
                """"
            )
            ;
    }
}

