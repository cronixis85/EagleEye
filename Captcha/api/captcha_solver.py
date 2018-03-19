import helpers_captcha
import helpers
from imutils import paths


def solve_captcha_test_images():
    CAPTCHA_IMAGE_FOLDER = "amazon_test_images"

    lb = helpers_captcha.load_captcha_model_labels()
    model = helpers_captcha.load_captcha_model()

    # Grab some random CAPTCHA images to test against.
    # In the real world, you'd replace this section with code to grab a real
    # CAPTCHA image from a live website.
    captcha_image_files = list(paths.list_images(CAPTCHA_IMAGE_FOLDER))
    # captcha_image_files = np.random.choice(captcha_image_files, size=(10,), replace=False)

    # loop over the image paths
    for image_file in captcha_image_files:
        helpers_captcha.solve_captcha_file(model, lb, image_file, False)

def solve_captcha_url(url):
    lb = helpers_captcha.load_captcha_model_labels()
    model = helpers_captcha.load_captcha_model()
    image = helpers.url_to_cv2_image(url)
    return helpers_captcha.solve_captcha_image(model, lb, image, False)