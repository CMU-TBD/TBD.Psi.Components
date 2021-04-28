# TBD.Psi.TransformationTree
COPYRIGHT(C) 2020 - Transportation, Bots, and Disability Lab - CMU
Code released under MIT.

## Funding
This work is partially funded under a grant from the National Institute on Disability, Independent Living, and Rehabilitation Research (NIDILRR grant number 90DPGE0003). NIDILRR is a Center within the Administration for Community Living (ACL), Department of Health and Human Services (HHS). The contents of this website do not necessarily represent the policy of NIDILRR, ACL, HHS, and you should not assume endorsement by the Federal Government.

## Overview
This hybrid Psi component allows you to create transformation tree that connects different frames of references and query the transformation betweem them. Here's an illustrative example:

```csharp
tree = new TransformationTree<string>();
tree.UpdateTransformation("parent", "child", new CoordinateSystem());
var result = tree.QueryTransformation("parent", "child")
```
You can also attach it in a Psi component to let it be updated by different `Emitter<CoordinateSystem>` and publish a copy of itself.
```csharp
treeComponent = new TransformationTreeComponent(pipeline, 1000, tree)
treeComponent.AddTransformationUpdateLink(anotherEmitter)
treeComponent.Write("tree", store);
```