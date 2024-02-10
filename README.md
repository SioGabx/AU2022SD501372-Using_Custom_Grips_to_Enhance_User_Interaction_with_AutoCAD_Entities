# Using Custom Grip to Enhance User Interaction with AutoCAD Entities

by Norman Yuan (https://drive-cad-with-code.blogspot.com/) / NormCadSoft


## Description
AutoCAD use grips showed with selected entity not only to provide visual hints of the entity, such start/end/center points, or mid/tangent points or vertices, but also more importantly provide shortcuts to various operations against the entity, such as stretch/move/rotation/scale. Other AutoCAD verticals (C3D/ARCH/MEP…) use custom grips with AEC entities extensively. 

Fortunately, with .NET API, we can add custom grip to AutoCAD entity for our own specialized operation against selected entity. This instruction shows a few practical examples of using custom grips

## Why Custom Grip
All AutoCAD users are familiar to “grip”, which appears when an AutoCAD entity is selected in AutoCAD editor. Grips not only shows geometrical significant information of the entity, such as end point, center, mid-point…, but also provides shortcut to some entity editing actions, such as dragging to stretch or move, or showing context menu for extra action options.

Besides standard grips that were available from earlier versions of AutoCAD, other types of grips were introduced into AutoCAD, such as the various grip types used by dynamic block, which are used for user to change block’s dynamic properties. Many AutoCAD verticals (C3D, Architecture, MEP…) use their custom grips extensively to provide better user experience when users interact with entities in AutoCAD editor.

GripOverrule exposed in AutoCAD .NET API makes it possible to us, as AutoCAD customization programmer, to create custom entity grips to allow CAD uses to interact with the entities easier, more efficient and more accurate.
Creating custom grips would involve 2 tasks:
• Create the grips visually, so they appear when the entities are selected
• Associate actions to the grip, so when use can interact with them (usually dragging, or clicking/right-clicking) to trigger certain changes/updates to the entities


### Read more : https://www.autodesk.com/autodesk-university/class/Using-Custom-Grip-Enhance-User-Interaction-AutoCAD-Entities-2022

## Ressources :
https://forums.autodesk.com/t5/net/gripdata-onhover-how-to-add-a-context-menu/m-p/5667518#M44869
https://adndevblog.typepad.com/files/sgp_overruletest_xdict.zip
https://www.theswamp.org/index.php?topic=49363.0
https://www.keanw.com/2009/08/knowing-when-an-autocad-object-is-grip-edited-using-overrules-in-net.html
https://forums.autodesk.com/t5/net/overrule-dynamic-block-grip-editing/m-p/3125022/highlight/true#M24726
https://forums.autodesk.com/t5/net/gripdata-onhover-how-to-add-a-context-menu/m-p/5667410#M44865
https://adndevblog.typepad.com/autocad/
https://through-the-interface.typepad.com/
