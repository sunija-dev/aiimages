# aiimag.es #
![GitHub](https://img.shields.io/github/license/GothaB/aiimages?style=for-the-badge)
![Discord](https://img.shields.io/discord/1012506307707809843?style=for-the-badge&logo=discord&logoColor=%237289DA)
![GitHub Repo stars](https://img.shields.io/github/stars/GothaB/aiimages?style=for-the-badge)

DOWNLOAD: [aiimag.es](https://aiimag.es)

<span style="color:orange">! This is a pre-release ! It is the code used for version 1.0.8 that you can download on the website (above). It is still a bit ugly, but if you want to use stable-diffusion in Unity, I guess this is your best option atm. <3 </span>

# Installation for Users #
1. Go to [aiimag.es](https://aiimag.es) and click "download"
2. Unzip it (at a path that has no space in it)
3. Run aiimages.exe

![aiimages tool](https://aiimag.es/wp-content/uploads/2022/09/aiimages_tool.jpg "aiimages tool")

# About #

## Goals ##
1. Make installation as easy as possible
2. Accessible to everyone
3. Easy to iterate on images

## Used Software ##
- Developed in Unity 2020.3.16f1
- Runs stable-diffusion v1.4 model
- Uses the [stable-diffusion implementation by LStein](https://github.com/lstein/stable-diffusion) as backend


# Future Features #

## Currently working on ##
1. History view
	1. Instead of generating 9 images, you just start generating and it adds the new pictures to the top of your history
2. Bugfixes

## Planned ##
1. Networking (aka "Borrow a GPU")
	1. Friends can connect to your PC and run their prompts on your GPU
	2. Using a custom solution, and Mirror and Light Reflective Mirror
2. Refactoring 
3. UI rework & missing features (inpainting, etc)
4. Android / WebGL versions (networking only)

## Wishlist ##
1. User managment (different users on one PC, different histories, ...)
2. Save style palettes and share them


# For Developers #

## Installation ##
1. !!! Install everything that follows on C:
2. Follow the [tutorial of Lstein](https://github.com/lstein/stable-diffusion) (don't worry, it's easy)
3. Open the repo in Unity 2020.3.16f1

## Deploying your solution ##
If you only change stuff on the Unity side:
1. Build your project
2. The downloads of dependencies should still work

If you also changed the conda environment:
1. (Coming soon. For now see [this](https://github.com/lstein/stable-diffusion/discussions/264#discussioncomment-3542992))

## How does it work interally? ##
1. The Unity project downloads all prerequisites (conda environment, model, python, sd-repo, ...) that were zipped with conda-pack
2. Unity starts an invisible command line to send the dream.py a prompts
3. ...once an image appears, Unity displays it

