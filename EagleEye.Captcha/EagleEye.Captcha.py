# -*- coding: utf-8 -*-
"""
Created on Sat Oct 28 12:27:57 2017

@author: Xianyang
"""

import pytesseract
import sys
import argparse
import os
import cv2
import numpy as numpy
from numpy import array
from PIL import Image
# from subprocess import check_output
 
 
os.chdir(os.path.join(os.getcwd(), "images"))
pytesseract.pytesseract.tesseract_cmd = 'C:\\Program Files (x86)\\Tesseract-OCR\\tesseract'
#path = 'NTJLBM.jpg'
 
# path = 'Captcha_rwspoebvrc.jpg'
 
# path = 'Captcha_srhrrhfzhn.jpg'
 
def captcha_solver(path): 
    image = cv2.imread(path)
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
     
    average = gray.mean(axis=0)
     
    pic_index = [i for i in range(len(average)) if average[i] < 255.0]
    min_val = pic_index[0]
    max_val = pic_index[-1]
    interval_width = (max_val - min_val) / 6
 
    for i in numpy.arange(0,6):
        if i==0:
            x = min_val
            y = int(round(x + interval_width))
            cv2.imwrite('pic.jpg', gray[:,x:y])
            img = Image.open('pic.jpg')
            # converted to have an alpha layer
            im2 = img.convert('RGBA')
            # rotated image
            rot = im2.rotate(15, expand=1)
            # a white image same size as rotated image
            fff = Image.new('RGBA', rot.size, (255,)*4)
            # create a composite image using the alpha layer of rot as a mask
            out = Image.composite(rot, fff, rot)
            tmpImg = str(i) + '.jpg'

            out.convert(img.mode).save(tmpImg)
            char = getImageText(tmpImg)                 
            string = char
        elif i in [2,4]:
            x = int(round(min_val + (i * interval_width)))
            y = int(round(x + interval_width))
            cv2.imwrite('pic.jpg', gray[:,x:y])
            img = Image.open('pic.jpg')
            # converted to have an alpha layer
            im2 = img.convert('RGBA')
            # rotated image
            rot = im2.rotate(15, expand=1)
            # a white image same size as rotated image
            fff = Image.new('RGBA', rot.size, (255,)*4)
            # create a composite image using the alpha layer of rot as a mask
            out = Image.composite(rot, fff, rot)
            tmpImg = str(i) + '.jpg';

            out.convert(img.mode).save(tmpImg)
            char = getImageText(tmpImg)                
            string += char
        else:
            x = int(round(min_val + (i * interval_width)))
            y = int(round(x + interval_width))
            cv2.imwrite('pic.jpg', gray[:,x:y])
            img = Image.open('pic.jpg')
            # converted to have an alpha layer
            im2 = img.convert('RGBA')
            # rotated image
            rot = im2.rotate(-15, expand=1)
            # a white image same size as rotated image
            fff = Image.new('RGBA', rot.size, (255,)*4)
            # create a composite image using the alpha layer of rot as a mask
            out = Image.composite(rot, fff, rot)
            tmpImg = str(i) + '.jpg';

            out.convert(img.mode).save(tmpImg)
            char = getImageText(tmpImg)       
            string += char
    return(string) 

def getImageText(path):
    return pytesseract.image_to_string(Image.open(path), config="--psm 10").upper()
    #return pytesseract.image_to_string(Image.open(path), config='-c tessedit_char_whitelist=ABCDEFGHIJKLMNOPQRSTUVWXYZ --psm 10').upper()

#print(captcha_solver(path))

print(getImageText("AKBFRA.jpg"))