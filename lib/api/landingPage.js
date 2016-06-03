var _ = require('lodash'),
    Promise = require('bluebird'),
    util = require('../util'),
    qs = require('querystring'),
    log = util.logger();

function LandingPage(marketo, connection) {
  this._marketo = marketo;
  this._connection = connection;
}

LandingPage.prototype = {
  getLandingPages: function(options) {
    var path = util.createAssetPath( 'landingPages.json' );
    options = _.extend({}, options, {
      _method: 'GET'
    });
    return this._connection.get(path, {data: options});
  },
};

module.exports = LandingPage;
