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

  describe('getFileStream', () => {
    it('should get file stream correctly', async () => {
      const endpoint = 'https://www.marketo.com';
      const token = 'token';
      const dataMock = 'file stream';

      nock(endpoint)
        .get('/')
        .reply(200, dataMock);
      
      const fileStream = await getFileStream(endpoint, 'activities', '123-567-', token);
      
      let data = '';
      fileStream
        .on('data', (chunk) => {
          data += chunk;
        })
        .on('end', () => {
          expect(data).to.equal(dataMock);
        });
    });
  });
});
