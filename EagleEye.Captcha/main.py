import os
import captcha_v2 as captchaSolver

imageDir = os.path.join(os.getcwd(), "images")

def solveImage(fileName):
    result = captchaSolver.captcha_solver(os.path.join(imageDir, fileName + ".jpg"), 245);
    print(fileName + " == " + result + ": " + str(fileName == result))

solveImage("ACYKRB")
solveImage("AKBFRA")
solveImage("BPKURG")
solveImage("CBTRUR")
solveImage("CKFEBX")
solveImage("CLMFFE")
solveImage("CRYNRM")
solveImage("EAAMGR")
solveImage("HERYME")
solveImage("HLGGNU")
solveImage("JPUYJP")
solveImage("JRMBJG")
solveImage("JTNLHN")
solveImage("KHNRFG")
solveImage("LEAMXK")
solveImage("LYGJHY")
solveImage("MXJAAG")
solveImage("NNPPHP")
solveImage("NTJLBM")
solveImage("PUFTXE")
solveImage("PXCACE")
solveImage("TEERXM")
solveImage("XNKHFY")
solveImage("YMEPAX")