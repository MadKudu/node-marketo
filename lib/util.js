var _ = require('lodash'),
    bunyan = require('bunyan'),
    config = require('../config/config'),
    logger, logConfig;

logConfig = {
  name: config.name,
  streams: [
    {
      level: 'warn',
      stream: process.stderr
    },
    {
      level: 'debug',
      stream: process.stderr
    }
  ]
};

module.exports = {
  createAssetPath: function joinPath(var_args) {
    var args = Array.prototype.slice.call(arguments);
    args.unshift('/asset'+config.api.version);
    return args.join('/');
  },

  createPath: function joinPath(var_args) {
    var args = Array.prototype.slice.call(arguments);
    args.unshift(config.api.version);
    return args.join('/');
  },

  createBulkPath: function joinPath(var_args) {
    var args = Array.prototype.slice.call(arguments);
    args.unshift('/../bulk'+config.api.version);
    return args.join('/');
  },

  logger: function() {
    if (!logger) {
      logger = bunyan.createLogger({name: config.name});
      if (process.env.NODE_MARKETO_LOG_LEVEL) {
        logger.level(bunyan.resolveLevel(process.env.NODE_MARKETO_LOG_LEVEL));
      }
    }
    return logger;
  },

  arrayToCSV: function(options, keys) {
    var copy = _.clone(options);
    _.each(keys, function(key) {
      if (_.isArray(copy[key])) {
        copy[key] = copy[key].join(',');
      }
    });
    return copy;
  },

  formatOptions: function(options, opt_filterArray) {
    options = _.clone(options);

    if (opt_filterArray) {
      options = _.pick(options, opt_filterArray);
    }

    return this.arrayToCSV(options, ['fields', 'filterValues']);
  }
};
