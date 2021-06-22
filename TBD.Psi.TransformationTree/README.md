# TBD.Psi.TransformationTree
COPYRIGHT(C) 2021 - Transportation, Bots, and Disability Lab - CMU
Code released under MIT.

## Funding
This work is partially funded under a grant from the National Institute on Disability, Independent Living, and Rehabilitation Research (NIDILRR grant number 90DPGE0003). NIDILRR is a Center within the Administration for Community Living (ACL), Department of Health and Human Services (HHS). The contents of this website do not necessarily represent the policy of NIDILRR, ACL, HHS, and you should not assume endorsement by the Federal Government.

## Overview
This Psi hybrid components enable you define a spatial relationship between multiple entities by specifying the transformation between them.
```csharp
var tree = new TransformationTree<string>();
var originalCoordinate = new CoordinateSystem(new Point3D(1, 2, 3), UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
tree.UpdateTransformation("parent", "child", originalCoordinate);
var newCoordinate = new CoordinateSystem(new Point3D(6, 5, 4), UnitVector3D.ZAxis, UnitVector3D.XAxis, UnitVector3D.YAxis);
tree.UpdateTransformation("parent", "child", newCoordinate);
```
A Psi component that wraps this capability is also provided
```csharp
using (var p = Pipeline.Create())
{
    var tree = new TransformationTreeComponent(p, 500);

    var t1 = Generators.Range(p, 1, 2, TimeSpan.FromSeconds(900));
    var t2 = Generators.Range(p, 5, 2, TimeSpan.FromSeconds(900));
    var t1cs = t1.Delay(TimeSpan.FromMilliseconds(900)).Select(m =>
    {
        var cs = new CoordinateSystem(new Point3D(m, 0, 0), UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
        return ("map", "r1", cs);
    });
    var t2cs = t2.Delay(TimeSpan.FromMilliseconds(900)).Select(m =>
    {
        var cs = new CoordinateSystem(new Point3D(0, m, 0), UnitVector3D.XAxis, UnitVector3D.YAxis, UnitVector3D.ZAxis);
        return ("map", "r2", cs);
    });

    tree.AddTransformationUpdateLink(t1cs);
    tree.AddTransformationUpdateLink(t2cs);

    tree.Do((m, e) =>
    {
        var cs = m.QueryTransformation("map", "r1");
    });
    p.Run(new ReplayDescriptor(DateTime.Now, DateTime.Now + TimeSpan.FromSeconds(2)));
```
## Changelog
#### 2021-06-21
- Initial release.
