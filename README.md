# UARK AR Colorblindness Experience
**University of Arkansas CSCE Project**
<br><br>
**Table of Contents:**<br>
&emsp;\* [Project Overview](#project-overview)<br>
&emsp;\* [Installation](#installation)<br>
&emsp;&emsp;- [Unity Version](#unity-version)<br>
&emsp;&emsp;- [Packages to run AR/MR](#ar-packages)<br>
&emsp;&emsp;- [Meta Quest Setup](#tritanopia-and-protanopia)<br>
&emsp;&emsp;- [Android Setup](#deuteranopia)<br>
&emsp;\* [Deployment](#deployment)<br>
&emsp;\* [Contributions](#contributions)<br>

---

# Project Overview

# Installation

## Unity Version
This project was built using the Unity Long Term Support (LTS) 2022 software. Ensure to download an install the current version of Unity by visiting [the Unity website](https://unity.com/releases/2022-lts).<br>

In addition, Android support is required for both Android and Meta Quest projects to be deployed. Ensure to downloaded by opening the `Unity Hub` application and navigating to:<br><br>
`Installs -> Unity 2022.X LTS (2022.X.XXXX) -> ⚙ Manage -> Add modules -> Android Build Support`<br><br>
Ensure to install the `OpenJDK` and `Android SDK & NDK Tools` if not automatically selected.<br><br>
>[!WARNING]
>While iOS is not supported, errors may arise if it is not installed. Therefore, it is recommended to install the iOS Build Support by navigating to:<br><br>
>`Installs -> Unity 2022.X LTS (2022.X.XXXX) -> ⚙ Manage -> Add modules -> Android Build Support`<br>

>[!TIP]
>At this stage, each respective Unity package should automatically install and configure any additional packages.

<br>Now create a new Universal Render Pipeline template project using the Unity Hub.

>[!CAUTION]
>Ensure that you select `Universal 3D` rather than `3D (Built-In Render Pipeline)`. Selection of the wrong project type may result in unexpected performance and issues.
<br>You are now able to begin configuring this project for AR/MR!

## AR Packages
To enable AR/MR, this project uses Unity's built-in XR plug-in, Google ARCore, and AR Foundation packages. Open the project to install these libraries.<br><br>

`Unity Plug-in` can be installed by going to `Edit -> Project Settings -> XR Plugin Management`. If the package is uninstalled, you will see this window:<br><br>

![Image showing XR Plugin Management not installed](https://github.com/cyetheguy/Uark_AR_Colorblindness_Experience/blob/main/media/no_xr_plugin_management.JPG)<br><br>

Pressing `Install XR Plugin Management` will install the necessities to run AR/MR. You should then see this screen:<br><br>

![Image showing XR Plugin Management installed](https://github.com/cyetheguy/Uark_AR_Colorblindness_Experience/blob/main/media/xr_plugin_managment.JPG)<br><br>

Navigate to the `<Android icon> -> Plug-in Providers -> Google ARCore` and check the box. Follow the steps to install the packages.

>[!TIP]
>You can verify that all packages are installed by going to `Window -> Package Manager` and verifying that "Google ARCore XR Plugin is installed (the button will way remove instead of install).

Next, install the AR Foundation package. This can be achieved by navigating to `Window -> Pakcage Manager -> Unity Registry -> AR Foundation`, and pressing the install button.<br>
## Tritanopia and Protanopia
These packages are designed for deployment on a Meta Quest 3. With a headset in [developer mode](https://developers.meta.com/horizon/documentation/native/android/mobile-device-setup/), install the following packages:
| Name | Unity Package |
| :--- | :--- |
| Meta XR Core SDK | [Unity Asset Store](https://assetstore.unity.com/packages/tools/integration/meta-xr-core-sdk-269169)|
| Meta Interactions SDK | [Unity Asset Store](https://assetstore.unity.com/packages/tools/integration/meta-xr-interaction-sdk-265014)|

>[!WARNING]
>There are two interaction packages: `Meta Interactions SDK` and `Meta Interactions SDK Essentials`. This project was developed using the `Meta Interactions SDK` package. Choosing the latter package may yield incorrect results.

Return to your project, and install the packages via `Window -> Package Manager -> <package>`. Follow any recommendations to complete setup for Meta Quest XR.<br>

## Deuteranopia
This project was designed to run on an Android device, and needs no installation of external packages. So take a break, and enjoy the birds! 🙂

# Deployment

This project is divided into three parts, one per developer. The following table can be used to download a Unity package to import the full project into the game:

| Minigame | Colorblind exploit | Deployment Platform | Package file|
| :--- | :--- | :--- | :--- |
| The Hidden Signal | Tritanopia | Meta Quest 3 | [Package](https://github.com/cyetheguy/Uark_AR_Colorblindness_Experience/blob/main/published/Hidden%20signal%20Finished_version.zip) |
| Sudoku | Protanopia | Meta Quest 3| [Package](https://github.com/cyetheguy/Uark_AR_Colorblindness_Experience/blob/main/published/TritanopiaShanakaHMD.unitypackage) |
| Snake | Deuteranopia | Android | [Package](https://github.com/cyetheguy/Uark_AR_Colorblindness_Experience/blob/main/published/colorblindExperience_Android.unitypackage) |

Download the desired package, and drop it into the [configured](#installation) project. This will automatically install and setup the project component.<br><br>

>[!IMPORTANT]
>If all else fails (either the `.unitypackage` or `.zip` file to setup), "It worked on my machine."<br>
>Each folder in this repository was a working project. To load in one of these projects, download the folder. Inside Unity Hub, add the project via:<br>
>`Add -> Add project from disk`<br>
>Select the directory (the root of where the `.unity` is), and open it. This will allow Unity to setup the project, and install its dependencies.
<br>

>[!TIP]
>Your project may not automatically configure the project to an Andoid platform. To fix this, go to `File -> Build Settings... -> Android -> Switch Platform`. Follow the prompts to switch the deployment platform to Android.

Select the device (Android or Meta Quest 3) you wish to deploy to in the dropdown in `Run Device`. (Press `Refresh` if your device is not listed).<br><br>
Select `Build and Run`, save your `.apk`, and the project component will be deployed to your device! Congrats!

# Contributions
| Name | GitHub Account |
| :--- | :--- |
| Caleb Young | [@cyetheguy](https://github.com/cyetheguy) |
| Diwash Pokharel | [@pokhareldiwash112](https://github.com/pokhareldiwash112) |
| Shanaka Edirisinghe | [@ahanakasampath](https://github.com/shanakasampath) |
