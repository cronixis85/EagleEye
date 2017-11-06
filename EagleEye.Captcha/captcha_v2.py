# -*- coding: utf-8 -*-
"""
Created on Mon Oct 30 00:25:12 2017

@author: Xianyang
"""

import sys
import base64
import io
import numpy as np
import argparse
import cv2
import os
from PIL import Image
import pytesseract

pytesseract.pytesseract.tesseract_cmd = 'C:\\Program Files (x86)\\Tesseract-OCR\\tesseract'
   
def captcha_solver(b64, threshold):

    ensure_dir(".tmp\\")

    image = convertBase64ToImage(b64)

    # image = cv2.resize(image, (400, 140), interpolation=cv2.INTER_CUBIC)
    image = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    
    colmean = image.sum(axis=0) / 70
    colmean_index = np.where(colmean < threshold)
    min_val = np.min(colmean_index)
    max_val = np.max(colmean_index)
    
    colmean_index = list(colmean_index)
    separators = []
    
    for i in np.arange(0,len(colmean_index[0]) - 1):
        if colmean_index[0][i] != colmean_index[0][i + 1] - 1:
            separators.append(colmean_index[0][i])

    if len(separators) == 5: 

        album = {
            1: image[:,min_val:separators[0]],
            2: image[:,separators[0] + 1:separators[1]],
            3: image[:,separators[1] + 1:separators[2]],
            4: image[:,separators[2] + 1:separators[3]],
            5: image[:,separators[3] + 1:separators[4]],
            6: image[:,separators[4] + 1:max_val]
        }
        
        string = ""

        for i in np.arange(1, 7):
            img = Image.fromarray(album[i])
            # converted to have an alpha layer
            im2 = img.convert('RGBA')

            # rotated image
            if i in [1, 3, 5]:
                rot = im2.rotate(15, expand=1)
            else:
                rot = im2.rotate(-15, expand=1)

            # a white image same size as rotated image
            fff = Image.new('RGBA', rot.size, (255,) * 4)
            # create a composite image using the alpha layer of rot as a mask
            out = Image.composite(rot, fff, rot)
            out.convert(img.mode).save('.tmp\\pic.jpg')
            char = runTesseract(Image.open('.tmp\\pic.jpg'))
            string += char

        string = string.replace("¥", "Y")

        return string
    else: 
        return ""

def convertBase64ToImage(b64):
    bytes = base64.b64decode(b64)
    nparr = np.fromstring(bytes, np.uint8)
    return cv2.imdecode(nparr, cv2.IMREAD_COLOR)

def runTesseract(img):
    return pytesseract.image_to_string(img, config='-c tessedit_char_whitelist=ABCDEFGHIJKLMNOPQRSTUVWXYZ -psm 10')[0].upper()

def ensure_dir(file_path):
    directory = os.path.dirname(file_path)
    if not os.path.exists(directory):
        os.makedirs(directory)

if __name__ == '__main__':
    print(captcha_solver(sys.argv[1], 245))