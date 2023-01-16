var offset = require('./offset');
var token = require('./token');
var util = require('../util');
var _ = require('lodash');

module.exports = getNextPageFn;

function getNextPageFn(conn, method, args) {
  var assetStrategy = {
    offset,
    token,
  };

  var options = _.clone(_.last(args) || {});
  args = _.clone(args);
  args.pop();

  var selectedStrategy = assetStrategy[util.nextPageType(args[0])];
  return selectedStrategy(conn, method, args, options);
}
