FreeImageDraw README

[github repo](https://github.com/ambid17/UnityImageDrawing "Github repo")

# Included Files
## Materials:
* TransparentImageMaterial- this material is included to show how to draw on top of other images, such that the image you are drawing on is transparent
## Resources:
* drawableImage- this image is the one you will be editing. It is a simple 1024X1024 png made in MS Paint
* orangeImage/blueImage- these are the background/foreground images showing how to order the images in a canvas to make one image render in front of another
## Scenes:
* DrawScene- a very simple scene showing the basic scene setup to get drawing to work properly
## Scripts:
* DrawViewController- the script where all of the drawing is done, this is attached to the gameObject you want to edit
* DrawSettings- this is the data model to hold the draw settings (i.e. draw color, line width, etc)
* README- this file

# Steps to get the drawing working:
1. Create a .png image in your favorite image editor (I simply used MS Paint)
2. Drop the new image into your project (preferably in the Assets/Resources folder)
3. select the image in the Assets folder and apply the following settings:
* Set "Texture Type" to "Sprite (2D and UI)"
* Set the "Pivot" to "BottomLeft"
* Check the box for "Read/Write Enable" 
* Set the "Compression" dropdown to "None"
* Hit "Apply"
4. Create an image in the scene (right click in scene heirarchy>UI>Image)
5. Set the Image's "Source Image" to be the image you just created (you can drop and drop from the assets folder)
6. Attach the "DrawViewController" script to the newly created Image
7. Hit play, and left click to draw
8. To undo/redo in the editor use "shift+z/shift+y" respectively. If you are testing this in a built version use "control+z/control+shift+Z"

# Troubleshooting:
## Image settings:
* check that the image you are using has "Texture Type" set to "Sprite (2D and UI)", and make sure you hit "Apply"
* check that the image you are using has the "Pivot" dropdown set to "BottomLeft", and make sure you hit "Apply"
* check that the image you are using has "Read/Write Enable" checked, and make sure you hit "Apply"
* check that the image you are using has the "Compression" dropdown set to "None", and make sure you hit "Apply"

## Runtime issues:
* if you change the image at runtime, make sure you call the Initialize() method
* make sure the RectTransform the image is attached to has a pivot of (0,0)
