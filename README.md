# TBD.Psi.Components
COPYRIGHT(C) 2021 - Transportation, Bots, and Disability Lab - CMU  
Code released under MIT.

# Funding
This work is partially funded under a grant from the National Institute on Disability, Independent Living, and Rehabilitation Research (NIDILRR grant number 90DPGE0003). NIDILRR is a Center within the Administration for Community Living (ACL), Department of Health and Human Services (HHS). The contents of this website do not necessarily represent the policy of NIDILRR, ACL, HHS, and you should not assume endorsement by the Federal Government.

# Overview
These are independent components developed by our lab for Psi Developements. They are built against the Psi Nuget packages instead of the main repository and might be uncompatible with the latest version on Github. Refer to the section below for how to develop for it.
|Name|Platform|TargetFramework|Detail|Nuget|
|---|---|---|---|---|
|[TBD.Psi.Imaging.Windows](TBD.Psi.Imaging.Windows/README.md)|Windows|net472|Helper Imaging Functions (faster JPEG encoding with LibJPEGTrubo)||
|[TBD.Psi.RosBagStreamReader](TBD.Psi.RosBagStreamReader/README.md)|Windows/Linux|netstandard2.0|Base PsiStream Reader for ROS Bags. Needs one of the two framework specific packages to work.|[0.1.0-beta](https://www.nuget.org/packages/TBD.Psi.RosBagStreamReader/)|
|[TBD.Psi.RosBagStreamReader.Windows](TBD.Psi.RosBagStreamReader.Windows/)|Windows|net472|A Windows specific PsiStream Reader for ROS Bags||
|[TBD.Psi.RosBagStreamReader.Windows.x64](TBD.Psi.RosBagStreamReader.Windows.x64/)|Windows x64|net472|A Windows x64 specific PsiStream Reader for ROS Bags||
|[TBD.Psi.RosSharpBridge.Windows](TBD.Psi.RosSharpBridge.Windows/README.md)|Windows|net472|A Psi wrapper for [ROS #](https://github.com/siemens/ros-sharp)||
|[TBD.Psi.TransformationTree](TBD.Psi.TransformationTree/README.md)|Windows/Linux|netstandard2.0|A spatial transformation frame representation.||
|[TBD.Psi.Sensors](TBD.Psi.Sensors/README.md)|Windows/Linux|netstandard2.0|A collections of classes that represent/operate on Sensor (e.g. LaserScan) data|[0.1.0-beta](https://www.nuget.org/packages/TBD.Psi.Sensors/)|

# Development
## Local Source vs Nuget Package.
From our experience, a mixing of Psi Nuget and locally reference Psi source packages always leads to build errors. This repostiory provides a `Local` configuration that references Psi source packages instead of the Nuget package. This is useful if your Psi framework is different from the latest version of the Psi Nuget packages. To switch between the references, change the project's configuration (local <-> debug).

# Contributions:
Contributions on bug fixes, additions, questions are always welcomed!
## Past Contributors:
- [Victor Del Carpio](https://github.com/victor-hugo-dc)
    - TBD.Psi.RosBagStreamReader
    - TBD.Psi.RosBagStreamReader.Windows.x64 
