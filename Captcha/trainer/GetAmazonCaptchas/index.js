const fs = require('fs');
const request = require('request');
const download = require('download');
const cheerio = require('cheerio');

const tmpDir = '.tmp';

if (!fs.existsSync(tmpDir)){
    fs.mkdirSync(tmpDir);
}

for (let i = 0; i < 1000; i++) {
    request('https://www.amazon.com/errors/validateCaptcha', function (error, response, body) {

        if (!error && response && response.statusCode == 200) {
            var $ = cheerio.load(body);
            var img = $('form img');
            var src = img.prop('src');

            download(src).then(data => {
                var filename = tmpDir + '/{0}.jpg'.replace('{0}', i);
                fs.writeFileSync(filename, data);
                console.log('added', filename);
            });

        } else {
            console.log('error:', error); // Print the error if one occurred
            console.log('statusCode:', response && response.statusCode); // Print the response status code if a response was received
            console.log('body:', body); // Print the HTML for the Google homepage.
        }       
    });
}