var _ = require('lodash'),
  config = require('../config'),
  logConfig;

logConfig = {
  name: config.name,
  streams: [
    {
      level: 'warn',
      stream: process.stderr,
    },
    {
      level: 'debug',
      stream: process.stderr,
    },
  ],
};

const createBulkPath = (...arguments) => {
  const args = Array.prototype.slice.call(arguments);
  args.unshift('/../bulk' + config.api.version);

  return args.join('/');
};

module.exports = {
  createAssetPath: function joinPath(var_args) {
    var args = Array.prototype.slice.call(arguments);
    args.unshift('/asset' + config.api.version);
    return args.join('/');
  },

  createPath: function joinPath(var_args) {
    var args = Array.prototype.slice.call(arguments);
    args.unshift(config.api.version);
    return args.join('/');
  },

  createBulkPath: createBulkPath,

  logger: function () {
    return console;
  },

  arrayToCSV: function (options, keys) {
    var copy = _.clone(options);
    _.each(keys, function (key) {
      if (_.isArray(copy[key])) {
        copy[key] = copy[key].join(',');
      }
    });
    return copy;
  },

  formatOptions: function (options, opt_filterArray) {
    options = _.clone(options);

    if (opt_filterArray) {
      options = _.pick(options, opt_filterArray);
    }

    return this.arrayToCSV(options, ['fields', 'filterValues']);
  },

  nextPageType: function (requestedUrl) {
    var type = 'token';
    if (requestedUrl.indexOf('asset') !== -1) {
      type = 'offset';
    }
    return type;
  },
};
