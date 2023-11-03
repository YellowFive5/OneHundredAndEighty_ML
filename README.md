# OneHundredAndEighty_ML

Detection, based on machine learning

## Contents
* [0. Why and what](https://github.com/YellowFive5/OneHundredAndEighty_ML#why)
* [1. Only one camera](https://github.com/YellowFive5/OneHundredAndEighty_ML#1-only-one-camera)
* [2. Some pre-calculations](https://github.com/YellowFive5/OneHundredAndEighty_ML#2-some-pre-calculations)
* [3. Synthetic data with blender scripting](https://github.com/YellowFive5/OneHundredAndEighty_ML#3-synthetic-data-with-blender-scripting)
* [4. .NET ML from box](https://github.com/YellowFive5/OneHundredAndEighty_ML#4-net-ml-from-box)
* [5. First results](https://github.com/YellowFive5/OneHundredAndEighty_ML#5-first-results)
* [Links and things](https://github.com/YellowFive5/OneHundredAndEighty_ML#links-and-things)

## Why?

It just so happened, that, because of many reasons, developement of my [OneHundredAndEighty](https://github.com/YellowFive5/OneHundredAndEighty) was on pause for more than half year. And now, with fresh look, I clearly see big problem - detection paradigm.

There are many people, who had stucked with camera setups to start using app. And I feel sorry for everyone, who failed with it.

2 years ago, when I start this project, it was experiment for me. But now, this 4 cams, this all geometry, this primitive triangulation, this white background, this total unstable and right detection rate in 90-95% looks awful for me...

## Modern world needs modern solutions

That's why I want to try ML. The idea is to have 1 camera with general dartboard view. Someway I need to collect a lot of images to create learning data set to learn neural network. Then, using this network, I will be able to get detection results.

![IMG](https://user-images.githubusercontent.com/42347722/114268242-d2ae8280-9a08-11eb-93a8-f24947e13dc6.jpg)

## 1. Only one camera

![Camera](https://github.com/YellowFive5/OneHundredAndEighty_ML/assets/42347722/ffb992f8-e117-4079-8994-4e07c464d5f3)

I use Logitech c920e

## 2. Some pre-calculations

![Mapping](https://github.com/YellowFive5/OneHundredAndEighty_ML/assets/42347722/5389fed4-ced1-477f-b32a-155dd7182637)

This simplyfied image shows how many throws needs to be done and captured to have full and good quality dataset.

It's obvious, that collecting such huge amount of data with hands is very long and difficult.

## 3. Synthetic data with blender scripting

I created blender scene with dartboard and camera position similar to my camera

![CameraScene jpeg](https://github.com/YellowFive5/OneHundredAndEighty_ML/assets/42347722/f54bec9a-c1b1-4902-a7e6-946f61ffe6b1)

![blender](https://github.com/YellowFive5/OneHundredAndEighty_ML/assets/42347722/7a4d09e9-f72d-44b7-b233-dd164a33316d)

I wrote some scripts to move dart it in different positions and render synthetic images.

To test how all works I start with only bull and 25 sectors.

![Renders](https://github.com/YellowFive5/OneHundredAndEighty_ML/assets/42347722/583d1715-4c58-4471-9d40-b5f4a40de37f)

I genereted 9k (28 Gb) images for bull sector and 53k (154 Gb) images for 25 sector. Then convered it to projection diff images for model learning

## 4. .NET ML from box
First thing, that I decided to try, was simple image classification using .NET ML libraries. I tried [this guide](https://analyticsindiamag.com/step-by-step-guide-for-image-classification-using-ml-net/)

So I has 62k images, marked with 2 types of labels (Bull or 25).

I wrote code to train model on images and save it.

Then I render 20 new images (10 bulls and 10 25's) to test results of learned models predictions.

![resnetTests](https://github.com/YellowFive5/OneHundredAndEighty_ML/assets/42347722/623cb8a7-d9ca-4edc-9b19-1c6dd2d74484)

ResnetV2101 model - 1000 Epochs - 0.05 fractions predict 100% 😎

So, raw results says, that idea of ML works overall, but we need more training dataset at first and more deep understanding of ML techniques and tips/tricks.

## 5. First results

Further tests show that everything is not so beautiful and simple (

I took 20 new images from camera (10 bulls and 10 25's) as test images for model. I hoped that model learned on synthetic data will be same accuracy for real images, but unfortnatelly, best results I get is:

ResnetV2101 model - 1000 Epochs - 0.05 fractions predict 14/20 on real images. 

Almost every error on bulls. I think, images from real cam has many more noise that renders and that is a problem.

...to be continued...

## Links and things
* [Led ring origin model](https://www.thingiverse.com/thing:4753272?fbclid=IwAR1P2s8mtMV7xil93jrYmAPycB94fgMHyx9JStwKNHWg2jZmfU5ge5BDYVE)

P.S. - If you like this stuff and if you want you can donate me for some beer 🍻 (click 💜"Sponsor" button in the top of a page)
