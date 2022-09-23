# aiimag.es #
![GitHub](https://img.shields.io/github/license/GothaB/aiimages?style=for-the-badge)
![Discord](https://img.shields.io/discord/1012506307707809843?style=for-the-badge&logo=discord&logoColor=%237289DA)
![GitHub Repo stars](https://img.shields.io/github/stars/GothaB/aiimages?style=for-the-badge)

DOWNLOAD: [aiimag.es](https://aiimag.es)
Download on itch.io: [aiimag.es](https://sunija.itch.io/aiimages)

# Installation for Users #
1. Go to [aiimag.es](https://aiimag.es) and click "download"
2. Unzip it (at a path that has no space in it)
3. Run aiimages.exe

![aiimages tool](https://img.itch.zone/aW1hZ2UvMTcwMTI3MC8xMDA2NzI1My5qcGc=/original/sLtztY.jpg "aiimages tool 1_1_0")

# About #

## Goals ##
1. Make installation as easy as possible
2. Accessible to everyone
3. Easy to iterate on images

## Features ##
- **Inpainting!** You got a broken face or a missing arm? Just paint over it in the app and let it be redrawn!
- **Variations!** Found an almost-good image? Lock the seed and create slight variations of it!
- **Better faces!** Got slightly broken face? Let the face AI update it!
- **Upscaling!** You fixed the face, the missing arms and got the right variation? Time to upscale it with just another AI!
- **Seamless option.** Wanna create beautiful tiling textures? Apply tiling!
- **History view.** Just go back to your old stuff!
- **Drag'n'drop.** Want to use a picture as input? Drag it there. Want to use a specific seed? Drag it on the seed. Etc.
- **Templates.** Save good style/content prompts and quickly load them. Comes with some pre-installed templates!
- **Open Source!** Yeah, I guess you already know that...

## Used Software ##
- Developed in Unity 2020.3.16f1 with C#
- Runs stable-diffusion v1.4 model
- Uses the [stable-diffusion implementation by LStein](https://github.com/lstein/stable-diffusion) as backend

# Future Features #

1. **Networking** (aka "Borrow a GPU")
	1. Friends can connect to your PC and run their prompts on your GPU
	2. Using a custom solution, and Mirror and Light Reflective Mirror
2. **Android / WebGL** versions (networking only)
3. **User/project managment.** Different users on one PC, different histories, ...
4. **Save/share style palettes.** Also just drop a generated image in to use it's style.
5. **Mac/Linux builds.** Should be easy with Unity, but I cannot test it at home. :(
6. **Multi-GPU support.** Start the backend twice, get twice as many pictures!
7. **GPU clustering.** Connect to friends via network, prompts are calculated on the free GPUs.
8. **Outpainting window.**
9. **UI improvements.** "Generate X pictures" button, better error feedback, ui animations,...
10. **Lexica.art wishlist.** Simply input nice prompts from lexica.art to test them later (and create templates from it).
11. **Stats view.** E.g. containing the GPU time, estimating power usage and costs, etc.

# For Developers #

## Installation ##
1. Alternative: Instead of installing the LStein environment, you could use the one from the build. But you'll have to edit GeneratiorConnection.cs (basically reverse the #ifdefs). Sorry that I didn't allow for that yet. :X
2. !!! Install everything that follows on C: 
3. Follow the [tutorial of Lstein](https://github.com/lstein/stable-diffusion) (don't worry, it's easy)
4. Open the repo in Unity 2020.3.16f1

## Deploying your solution ##
If you only change stuff on the Unity side:
1. Build your project
2. The downloads of dependencies should still work

If you also changed the conda environment:
1. (Coming soon. For now see [this](https://github.com/lstein/stable-diffusion/discussions/264#discussioncomment-3542992))

## How does it work interally? ##
1. The Unity project is zipped with all prerequisites: conda environment (with conda-pack), model, python, sd-repo, ai cache.
2. Unity starts an invisible command line, runs the dream.py and sends it prompts
3. ...once an image appears, Unity displays it

