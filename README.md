## Contents
* [0. Why and what](https://github.com/YellowFive5/OneHundredAndEighty_ML#why)
* [1. Some pre-calculations](https://github.com/YellowFive5/OneHundredAndEighty_ML#1-some-pre-calculations)
* [2. Only one camera](https://github.com/YellowFive5/OneHundredAndEighty_ML#2-only-one-camera)
* [3. DotsMapper](https://github.com/YellowFive5/OneHundredAndEighty_ML#3-dotsmapper)
* [4. DiffImagesCollector](https://github.com/YellowFive5/OneHundredAndEighty_ML#4-diffimagescollector)

# OneHundredAndEighty_ML

Detection, based on machine learning

## Why?

It just so happened, that, because of many reasons, developement of my [OneHundredAndEighty](https://github.com/YellowFive5/OneHundredAndEighty) was on pause for more than half year. And now, with fresh look, I clearly see big problem - detection paradigm.

There are many people, who had stucked with camera setups to start using app. And I feel sorry for everyone, who failed with it.

2 years ago, when I start this project, it was experiment for me. But now, this 4 cams, this all geometry, this primitive triangulation, this white background, this total unstable and right detection rate in 90-95% looks awful for me...

## Modern world needs modern solutions

That's why I want to try ML. The idea is to have 1 camera with general dartboard view. In prepare-game process I need to collect many-many diff-images to create learning data set to learn neural network. Then, using this network, I will be able to get detection results.

![IMG](https://user-images.githubusercontent.com/42347722/114268242-d2ae8280-9a08-11eb-93a8-f24947e13dc6.jpg)

I had some consultations with men who use ML on in his work and knows many about - he said it's smart idea and I must succeed with it.

So let's go...

## 1. Some pre-calculations
![Mapping](https://user-images.githubusercontent.com/42347722/114311826-fc46d700-9af8-11eb-926e-806838f7d4d0.jpg)

This simplyfied map shows how many throws can be done. This is simple-minimum. On all this throws I need minimum one diff image to learn. From this calculations becomes clear, that it's a lot work ahead... too lot.

So, it will be right to start small. I'll try to prepare minimum dataset for 25 and bulls to learn.

## 2. Only one camera
![IMG_2443](https://user-images.githubusercontent.com/42347722/114311813-f0f3ab80-9af8-11eb-99e9-6eacbfecab15.jpeg)

Camera case and mount printed and set.

## 3. DotsMapper
Further calculation process will use synthetic 1300x1300 px drawed dartboard image and POI's with it's coordinates. To collect this coordinates for learning and mapping to diff-image for NN I use [DotsMapper](https://github.com/YellowFive5/OneHundredAndEighty_ML/tree/main/DotsMapper) And that how it goes:

![2021-04-14 16_05_20-DotsMapper](https://user-images.githubusercontent.com/42347722/114715164-749cda80-9d3b-11eb-8573-dd5fb24479d8.png)

21 bull points and 108 _25 points at all.

All 129 points collected to local SQLite.db

## 4. DiffImagesCollector
![2021-04-25 14_12_05-DiffImagesCollector](https://user-images.githubusercontent.com/42347722/115991400-e04f3500-a5d0-11eb-8513-81911357cad2.png)
With [this simple util](https://github.com/YellowFive5/OneHundredAndEighty_ML/tree/main/DiffImagesCollector) I get next point from .db and it shows on projection. So I need to manually stick dart in this point and get capture. Diff images will save in filesystem and .db 

I plan to take some 'clear' diff-captures for each point. After that I will have small dataset to try to learn NN.

## 5. Collecting dataset
![IMG_2458](https://user-images.githubusercontent.com/42347722/116810408-506b3700-ab4c-11eb-80bb-cb4b053b9688.jpeg)

![2021-05-02 13_40_08-Images](https://user-images.githubusercontent.com/42347722/116810461-94f6d280-ab4c-11eb-83ed-6a7da7f0e0a9.png)

516 images (x4 for each point) collected. Now it's time to try to learn model.

...to be continued...

P.S. - If you like this stuff and if you want you can donate me for some beer 🍻 (click 💜"Sponsor" button in the top of a page)
