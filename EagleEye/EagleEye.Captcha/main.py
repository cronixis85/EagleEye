import os
import base64
import captcha_v2 as captchaSolver

imageDir = os.path.join(os.getcwd(), "images")

def solveImageFile(fileName):
    imagePath = os.path.join(imageDir, fileName + ".jpg")
    b64 = base64.b64encode(open(imagePath, "rb").read()).decode()
    result = solveBase64(b64)
    #print(fileName + " == " + result + ": " + str(fileName == result))
    return solveBase64(b64)

def solveBase64(b64):
    return captchaSolver.captcha_solver(b64, 245)

#solveImageFile("ACYKRB")
#solveImageFile("AKBFRA")
#solveImageFile("BPKURG")
#solveImageFile("CBTRUR")
#solveImageFile("CKFEBX")
#solveImageFile("CLMFFE")
#solveImageFile("CRYNRM")
#solveImageFile("EAAMGR")
#solveImageFile("HERYME")
#solveImageFile("HLGGNU")
#solveImageFile("JPUYJP")
#solveImageFile("JRMBJG")
#solveImageFile("JTNLHN")
#solveImageFile("KHNRFG")
#solveImageFile("LEAMXK")
#solveImageFile("LYGJHY")
#solveImageFile("MXJAAG")
#solveImageFile("NNPPHP")
#solveImageFile("NTJLBM")
#solveImageFile("PUFTXE")
#solveImageFile("PXCACE")
#solveImageFile("TEERXM")
#solveImageFile("XNKHFY")
#solveImageFile("YMEPAX")