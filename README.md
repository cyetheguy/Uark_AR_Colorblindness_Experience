# UARK AR Colorblindness Experience
**University of Arkansas CSCE Project**
<br><br>
**Table of Contents:**<br>
&emsp;\* [Project Overview](#project-overview)<br>
&emsp;\* [Installation](#installation)<br>
&emsp;&emsp;- [Unity Version](#unity-version)<br>
&emsp;&emsp;- [Packages to run AR/MR](#ar-packages)<br>
&emsp;&emsp;- [Meta Quest Setup](#tritanopia-and-protanopia)<br>
&emsp;&emsp;- [Android Setup](#protanopia)<br>
&emsp;&emsp;- [Deuteranopia](#deuteranopia)<br>
&emsp;\* [Deployment](#deployment)<br>
&emsp;&emsp;- [Android](#android)<br>
&emsp;&emsp;- [Meta Quest 3](#meta-quest-3)<br>
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

![Image showing XR Plugin Management not installed[()<br><br>

Pressing `Install XR Plugin Management` will install the necessities to run AR/MR. You should then see this screen:<br><br>

![Image showing XR Plugin Management installed[()<br><br>

Navigate to the `\<Android icon\> -> Plug-in Providers -> Google ARCore` and check the box. Follow the steps to install the packages.

>[!TIP]
>You can verify that all packages are installed by going to `Window -> Package Manager` and verifying that "Google ARCore XR Plugin is installed (the button will way remove instead of install).

Next, install the AR Foundation package. This can be achieved by navigating to `Window -> Pakcage Manager -> Unity Registry -> AR Foundation`, and pressing the install button.
## Tritanopia and Protanopia


## Deuteranopia

# Deployment

## Android

## Meta Quest 3

# Contributions
