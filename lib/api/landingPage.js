var _ = require("lodash"),
  Promise = require("bluebird"),
  util = require("../util"),
  qs = require("querystring"),
  log = util.logger();

function LandingPage(marketo, connection) {
  this._marketo = marketo;
  this._connection = connection;
}

LandingPage.prototype = {
  get: function (id, draft) {
    var path = util.createAssetPath(`landingPage/${id}.json`);
    if (draft) {
      path += `?status=draft`;
    }

    var options = _.extend(
      {},
      {
        _method: "GET",
      }
    );
    return this._connection.get(path, { data: options });
  },
  approveDraft: function (id) {
    var path = util.createAssetPath(`landingPage/${id}/approveDraft.json`);

    var options = _.extend(
      {},
      {
        _method: "POST",
      }
    );
    return this._connection.post(path, { data: options });
  },
  getLandingPages: function (options) {
    var path = util.createAssetPath("landingPages.json");
    options = _.extend({}, options, {
      _method: "GET",
    });
    return this._connection.get(path, { data: options });
  },
};

module.exports = LandingPage;
