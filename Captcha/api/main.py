import io
import captcha_solver
from flask import Flask, request, send_file

app = Flask(__name__)

@app.route('/health')
def health():
  return 'OK'

@app.route('/', methods = ['POST'])
def solve():
  captcha_text = captcha_solver.solve_captcha_url(request.form['url'])
  return captcha_text

if __name__ == '__main__':
  app.run()