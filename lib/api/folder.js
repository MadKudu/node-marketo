var _ = require('lodash'),
  Promise = require('bluebird'),
  util = require('../util'),
  qs = require('querystring'),
  log = util.logger();

function Folder(marketo, connection) {
  this._marketo = marketo;
  this._connection = connection;
}

Folder.prototype = {
  getByName: function (name, isProgram, root, workSpace) {
    var path = util.createAssetPath(`folder/byName.json`);
    if (name || isProgram || root || workSpace) {
      path = `${path}?`;
    }

    if (name) {
      path = `${path}name=${encodeURIComponent(name)}&`;
    }

    if (isProgram) {
      path = `${path}type=Program&`;
    }

    if (root) {
      path = `${path}root=${encodeURIComponent(root)}&`;
    }

    if (workSpace) {
      path = `${path}workSpace=${encodeURIComponent(workSpace)}`;
    }
    options = _.extend(
      {},
      {
        _method: 'GET',
      }
    );
    return this._connection.get(path, { data: options });
  },
  getById: function (folderId, isProgram) {
    var path = util.createAssetPath(`folder/${folderId}.json`);
    if (isProgram) {
      path = `${path}?type=Program`;
    }
    options = _.extend(
      {},
      {
        _method: 'GET',
      }
    );
    return this._connection.get(path, { data: options });
  },
  getContent: function (folderId, isProgram, maxReturn, offset) {
    var path = util.createAssetPath(`folder/${folderId}/content.json`);
    if (isProgram || maxReturn || offset) {
      path = `${path}?`;
    }
    if (isProgram) {
      path = `${path}type=Program`;
    }
    if (maxReturn) {
      path = `${path}maxReturn=${encodeURIComponent(maxReturn)}`;
    }
    if (offset) {
      path = `${path}offset=${encodeURIComponent(offset)}`;
    }
    options = _.extend(
      {},
      {
        _method: 'GET',
      }
    );
    return this._connection.get(path, { data: options });
  },
  getTokens: function (folderId, isProgram) {
    var path = util.createAssetPath(`folder/${folderId}/tokens.json`);
    if (isProgram) {
      path = `${path}?folderType=Program`;
    }
    options = _.extend(
      {},
      {
        _method: 'GET',
      }
    );
    return this._connection.get(path, { data: options });
  },
  deleteToken: function (folderId, tokenName, tokenType, isProgram) {
    var path = util.createAssetPath(`folder/${folderId}/tokens/delete.json`);
    options = _.extend(
      {},
      {
        folderType: isProgram ? 'Program' : 'Folder',
        name: tokenName,
        type: tokenType,
      },
      {
        _method: 'POST',
      }
    );
    return this._connection.post(path, { data: options });
  },
};

module.exports = Folder;
