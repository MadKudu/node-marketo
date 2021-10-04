const expect = require('chai').expect;
const nock = require('nock');

const { createBulkPath, getFileStream } = require('../lib/util');

describe('util', () => {
  describe('getFileStream', () => {
    it('should return base path with no arguments passed', () => {
      const path = createBulkPath();

      expect(path).to.equal('/../bulk/v1');
    });

    it('should create a bulk path correctly', () => {
      const path = createBulkPath('activities', 'export', 'create.json');

      expect(path).to.equal('/../bulk/v1/activities/export/create.json');
    });
  });
});
