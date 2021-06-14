# TBD.Psi.Components
COPYRIGHT(C) 2020 - Transportation, Bots, and Disability Lab - CMU
Code released under MIT.

# Funding
This work is partially funded under a grant from the National Institute on Disability, Independent Living, and Rehabilitation Research (NIDILRR grant number 90DPGE0003). NIDILRR is a Center within the Administration for Community Living (ACL), Department of Health and Human Services (HHS). The contents of this website do not necessarily represent the policy of NIDILRR, ACL, HHS, and you should not assume endorsement by the Federal Government.

# Overview
These are independent components developed by our lab for Psi Developements. They are built against the Psi Nuget packages instead of the main repository and might be uncompatible with the latest version on Github.
|Name|Platform|TargetFramework|Detail|
|---|---|---|---|
|[TBD.Psi.Imaging.Windows](TBD.Psi.Imaging.Windows/README.md)|Windows|net472|Helper Imaging Functions (faster JPEG encoding with LibJPEGTrubo)|
|[TBD.Psi.RosBagStreamReader](TBD.Psi.RosBagStreamReader/README.md)|Windows/Linux|netstandard2.0|A PsiStream Reader for ROS Bags|
|[TBD.Psi.TransformationTree](TBD.Psi.TransformationTree/README.md)|Windows/Linux|netstandard2.0| A hybrid component to register connected transformations in a scene. useful for scenarios with multiple frame of references.|
# ChangeLog
#### 2021-04-24
- Updated components to the latest version of Psi `0.15.49.1-beta` & fix compatibility problems.
- **[RosBagStreamReader]** Added additional tests to test reading streams from store using the same pattern as PsiStore.
#### 2021-04-04
- Added TransformationTree that keep tracks of relational transformations between coordinate frames.
- Added multiple deserializers for geometry_msgs and also converts tf to TransformationTree
#### 2020-12-15
- Updated the components to the latest version of Psi `0.14.35.3-beta` & fix compatibility problems.
- **[RosBagStreamReader]** Added additional tests.
