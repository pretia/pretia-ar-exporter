# Unity Exporter

The **Pretia Exporter** is a Unity Editor extension that allows creators to export complex asset setups directly into their Pretia AR asset library. This tool streamlines the process of preparing and uploading assets such as particle systems, animated prefabs, UI elements, and more, enabling richer, more dynamic AR experiences.

Please refer to the Pretia AR Docs for further information about the Pretia Exporter and Pretia AR: https://docs.pretiaar.com/

## Setup

1. Install Unity version 2021.3.x
    - Use the [Unity Hub](https://unity.com/download) or download manually.
    - We recommend version [2021.3.41f1](https://unity.com/releases/editor/whats-new/2021.3.41)
      - If Unity Hub is installed, you can install it directly by clicking: [Install 2021.3.41f1](unityhub://2021.3.41f1/6c5a9e20c022)

2. Create a new Unity project, or open an existing project
    - Ensure the project uses the Built-In Render Pipeline.
    - For new projects, we recommend using the default `3D (Built-In Render Pipeline)` template.

![Unity New Project](Img/unity_project_template.jpg)

3. **Add the Pretia AR Exporter package**
   - You can either:
      - [Download and import the `.unitypackage` file](https://creator.pretiaar.com/downloads/pretia-ar-exporter-latest.unitypackage), or
      - Add the package via the Unity Package Manager:
         - Click the `+` icon in the top-left corner of the Package Manager window, and select `Add package from git URL...`
         - Enter the following URL: `https://github.com/pretia/pretia-ar-exporter.git`

![Unity New Project](Img/import_package.jpg)

4. **Export your assets for Pretia AR**
   - Follow the guide here: [Exporting Your First Asset](https://docs.pretiaar.com/docs/unity-exporter/exporting-your-first-asset)

## Samples

The following sample assets are available as a reference, and for testing the exporter: [Download Sample Assets](https://creator.pretiaar.com/downloads/pretia-ar-exporter-latest.unitypackage)

Sample documentation is available here: [Sample Docs](https://creator.test.pretiaar.com/test/docs/category/samples)

## Notes

Thanks to needle-tools for their unity-deeplink project, which has been used to streamline Pretia's authentication system. You can find the original repository here:

https://github.com/needle-tools/unity-deeplink ([LICENSE](https://github.com/needle-tools/unity-deeplink/blob/main/package/Editor/Plugins/Needle.Deeplink.Harmony.License.md))