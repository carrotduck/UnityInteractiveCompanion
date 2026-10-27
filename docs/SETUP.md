# Open the demo

## 1. Open the project

Open the repository folder as a project in **Unity 6000.6.5f1**. Allow the Package Manager to restore the dependencies.

The Starter Assets 2.0.1 character, locomotion animations, materials and textures are included with their original asset GUIDs.

Choose **Window > TextMeshPro > Import TMP Essential Resources** to add the font and text resources used by the scene.

## 2. Greeting animation

On [Mixamo](https://www.mixamo.com/), select the **Waving** animation on the default character. The demo uses a 142-frame, 30 fps clip (4.733 seconds). Download **FBX Binary**, **Without Skin**, **30 fps**, and **no keyframe reduction**.

Close Unity, then run this from the repository root, replacing the path with your downloaded file:

```powershell
powershell -ExecutionPolicy Bypass -File tools/Restore-Wave.ps1 -WaveFile "C:\Downloads\Waving.fbx"
```

This copies the download to `Assets/CharacterAnimations/Waving.fbx` alongside the saved import settings. Reopen Unity to import it as a Humanoid clip. The settings preserve the clip name, root-motion configuration and GUID used by the custom animation controller.

## 3. Scene check

Open `Assets/Scenes/SampleScene.unity`.

- On `Companion`, keep the NavMeshAgent enabled and set **Character Animator** to the child `Armature` Animator.
- On `Armature`, use **CompanionHuman** as the Controller and keep **Apply Root Motion** disabled.
- In the `Waving` state, confirm the Motion field points to the imported **Waving** clip. If the downloaded animation uses a different internal clip ID, drag its green animation subasset into this field.
- If the navigation surface needs refreshing, select `Environment` and bake its NavMeshSurface.

Press Play. Approach from beside the wall to start the greeting. Accept the invitation, stay behind to trigger waiting, and approach again to resume guiding.

## Demo parameters

| Setting | Value |
| --- | --- |
| Move Speed | 2 |
| Notice Distance | 3 |
| Stopping Distance | 1.5 |
| Personal Distance | 1 |
| Greeting Hold Duration | 5.1 |
| Guide Wait Distance | 3.5 |
| Guide Resume Distance | 2.5 |
