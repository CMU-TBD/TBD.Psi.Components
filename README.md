# TBD.Psi.Components
COPYRIGHT(C) 2021 - Transportation, Bots, and Disability Lab - CMU  
Code released under MIT.

# Funding
This work is partially funded under a grant from the National Institute on Disability, Independent Living, and Rehabilitation Research (NIDILRR grant number 90DPGE0003). NIDILRR is a Center within the Administration for Community Living (ACL), Department of Health and Human Services (HHS). The contents of this website do not necessarily represent the policy of NIDILRR, ACL, HHS, and you should not assume endorsement by the Federal Government.

# Overview
These are independent components developed by our lab for Psi Developements. They are built against the Psi Nuget packages instead of the main repository and might be uncompatible with the latest version on Github.
|Name|Platform|TargetFramework|Detail|
|---|---|---|---|
|[TBD.Psi.Imaging.Windows](TBD.Psi.Imaging.Windows/README.md)|Windows|net472|Helper Imaging Functions (faster JPEG encoding with LibJPEGTrubo)|
|[TBD.Psi.RosBagStreamReader](TBD.Psi.RosBagStreamReader/README.md)|Windows/Linux|netstandard2.0|Base PsiStream Reader for ROS Bags. Needs one of the two framework specific packages to work.|
|[TBD.Psi.RosBagStreamReader.NET](TBD.Psi.RosBagStreamReader.NET)|Windows/Linux|net5.0|A .NET 5.0 PsiStream Reader for ROS Bags|
|[TBD.Psi.RosBagStreamReader.Windows](TBD.Psi.RosBagStreamReader.Windows/)|Windows|net472|A Windows specific PsiStream Reader for ROS Bags|
|[TBD.Psi.RosSharpBridge.Windows](TBD.Psi.RosSharpBridge.Windows/README.md)|Windows|net472|A Psi wrapper for [ROS #](https://github.com/siemens/ros-sharp)|
|[TBD.Psi.TransformationTree](TBD.Psi.TransformationTree/README.md)|Windows/Linux|netstandard2.0|A spatial transformation frame representation.|

# Development
## Local Source vs Nuget Package.
From our experience, a mixing of Psi Nuget and locally reference Psi source packages always leads to build errors. This repostiory provides a `Local` configuration that references Psi source packages instead of the Nuget package. This is useful if your Psi framework is different from the latest version of the Psi Nuget packages. To switch between the references, change the project's configuration (local <-> debug).