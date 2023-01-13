var _ = require('lodash'),
  util = require('util'),
  http = require('http');


function writeResponse(res, statusCode, data) {
  data = JSON.stringify(data);
  res.writeHead(statusCode, {
    'Content-Lenght': data.length,
    'Content-Type': 'application/json'
  });
  res.write(data); res.end();
}

function TestServer() {
  this._server = http.createServer(_.bind(this._handler, this));
  this._counter = {};
}

TestServer.prototype = {
  listen: function (cb) {
    this._server.listen(0, 'localhost', cb);
  },

  http: function () {
    return this._server;
  },

  // This is a catch all handler for the test http server and it keeps
  // track of how many times each path is hit. At the moment, it does not
  // distinguish between different HTTP methods, but there's not yet a
  // need for that.
  _handler: function testServer(req, res) {
    var statusCode = 200,
      data = {};

    this._counter[req.url] = (this._counter[req.url] || 0) + 1;

    // will close the connection forcefully (ECONNRESET)
    if (req.headers['connection-abort'] == 'true') {
      req.connection.destroy();
      return;
    }

    // responds with the specific status code
    if (_.has(req.headers, 'status-code')) {
      statusCode = req.headers['status-code'];
      writeResponse(res, req.headers['status-code'], data);
      return;
    }

    // simulates marketo error codes
    if (_.has(req.headers, 'marketo-error-code')) {
      data = {
        requestId: 'some-id',
        success: false,
        errors: [
          {
            message: 'Error message',
            code: req.headers['marketo-error-code']
          }
        ]
      };
      writeResponse(res, 200, data);
      return;
    }

    // Set an arbitrary delay until the server responds, by default there is
    // no delay.
    setTimeout(function () {
      writeResponse(res, 200, data);
    }, req.headers['delay'] || 0);
  },

  // Gets all the counts of all the URLs hit
  counts: function () {
    return _.cloneDeep(this._counter);
  },

  // Get the count of a specific path
  count: function (k) {
    return this._counter[k] || 0;
  },

  // Get the count of all urls that match the regex
  countPattern: function (regex) {
    const counts = this.counts()
    return _.reduce(_.keys(counts), function (sum, k) {
      if (k.match(regex)) {
        return sum + counts[k];
      }

      return sum;
    }, 0, this);
  }
};

module.exports = {
  newServer: function newServer(next) {
    var server = new TestServer();
    server.listen(next);
    return server;
  }
};
