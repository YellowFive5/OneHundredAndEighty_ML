import bpy
import uuid

def render(value):
    output_file = "Z:/Synthetic/bull/" + value + "_" + str(uuid.uuid4()) + ".png"
    bpy.context.scene.render.resolution_x = 1920
    bpy.context.scene.render.resolution_y = 1080
    bpy.context.scene.render.image_settings.file_format = "PNG"
    bpy.ops.render.render(write_still=True)
    bpy.data.images["Render Result"].save_render(output_file)

def moveXYClock(clock, times = 1): #12 2 4 6 8 10
    for x in range(times):
        if clock == 12:
            bpy.ops.transform.translate(value=(0, 0.024, 0))
        elif clock == 2:
            bpy.ops.transform.translate(value=(0.0208, 0.012, 0))
        elif clock == 4:
            bpy.ops.transform.translate(value=(0.0208, -0.012, 0))
        elif clock == 6:
            bpy.ops.transform.translate(value=(0, -0.024, 0))
        elif clock == 8:
            bpy.ops.transform.translate(value=(-0.0208, -0.012, 0))
        elif clock == 10:
            bpy.ops.transform.translate(value=(-0.0208, 0.012, 0))

def center(): # center
    bpy.ops.view3d.snap_selected_to_cursor(use_offset=True)

def stick(deep, times = 1):
    for x in range(times):
        bpy.ops.transform.translate(value=(0, 0, deep))

def rotateY(rads, times = 1):
    for x in range(times):
        bpy.ops.transform.rotate(value=rads, orient_axis='Y')
        
def rotateX(rads, times = 1):
    for x in range(times):
        bpy.ops.transform.rotate(value=rads, orient_axis='X')
        
def rotateZ(rads, times = 1):
    for x in range(times):
        bpy.ops.transform.rotate(value=rads, orient_type='LOCAL', orient_axis='Z')
        
# _________________________________________________________________________

renders = 0;
movesXY = [12,12,4,4,8]
yxRotatesAng = -0.05236
yRotatesTimes = 20
xRotatesTimes = 15
zRotatesAng = 0.392699
zRotatesTimes = 2

for x in movesXY:
    bpy.data.objects["Dart_2"].rotation_euler = (0,0,0)
    bpy.ops.transform.rotate(value=0.5236, orient_axis='Y')
    bpy.ops.transform.rotate(value=0.7854, orient_axis='X')
    for zr in range(zRotatesTimes):
        for yr in range(yRotatesTimes):
            for xr in range(xRotatesTimes):
                render("bull")
                renders = renders + 1
                print(str(renders) + "/" + str(len(movesXY) * yRotatesTimes * xRotatesTimes * zRotatesTimes))
                rotateX(yxRotatesAng)
            rotateX(yxRotatesAng * xRotatesTimes * -1)
            rotateY(yxRotatesAng)
        rotateY(yxRotatesAng * yRotatesTimes * -1)
        rotateZ(zRotatesAng)
    rotateZ(zRotatesAng * zRotatesTimes * -1)
    moveXYClock(x)

bpy.ops.transform.rotate(value=-0.7854, orient_axis='X')
bpy.ops.transform.rotate(value=-0.5236, orient_axis='Y')
bpy.data.objects["Dart_2"].rotation_euler = (0,0,0)