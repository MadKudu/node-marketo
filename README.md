# node-marketo

This implements (a subset of) Marketo's REST API.

## Overview

This library is a simple wrapper around Marketo's REST API and does not aim to be anything more than that.

## Usage

### Creating a connection

You will first need to obtain your OAuth information from Marketo, they have a [guide out](http://developers.marketo.com/documentation/rest/authentication/) to get you started. In short, you will need to get the endpoint url, identity url, client id and client secret.

```js
var Marketo = require('node-marketo');

var marketo = new Marketo({
  endpoint: 'https://123-ABC-456.mktorest.com/rest',
  identity: 'https://123-ABC-456.mktorest.com/identity',
  clientId: 'client id',
  clientSecret: 'client secret'
});

marketo.lead.find('id', [2, 3])
  .then(function(data, resp) {
    // data is:
    // {
    //   requestId: '17787#149c01d54b8',
    //   result: [{
    //     id: 2,
    //     updatedAt: '2014-11-13 15:25:36',
    //     createdAt: '2014-11-13 15:25:36',
    //     ...
    //   }, {
    //     id: 3,
    //     updatedAt: '2014-11-13 16:22:03',
    //     createdAt: '2014-11-13 16:22:03',
    //     ...
    //   }],
    //   success: true
    // }
  });
```

### Pagination

When a specific call results in a lot of elements, you will have to paginate to get all of the data. For example, getting all the leads in a large list will likely exceed the maximum batch size of 300. When this happens, you can check for the existence of the `nextPageToken` and use it in the next call:

```js
marketo.list.getLeads(1)
  .then(function(data) {
    if (data.nextPageToken) {
      // preserve the nextPageToken for use in the next call
    }
  });
```

If you want a less manual process, the result comes with a convenient `nextPage` function that you can use. This function only exists if there's additional data:

```js
marketo.list.getLeads(1)
  .then(function(page1) {
    // do something with page1

    if (data.nextPageToken) {
      return page1.nextPage();
    }
  })
  .then(function(page2) {
    // do something with page2
  });
```

### Stream

Instead of getting data back inside of the promise, you can also turn the response into a stream by using the `Marketo.streamify` function. This will emit the results one element at a time. If the result is paginated, it will lazily load all of the pages until it's done:

```js
// Wraping the resulting promise
var resultStream = Marketo.streamify(marketo.list.getLeads(1))

resultStream
  .on('data', function(lead) {
    // do something with lead
  })
  .on('error', function(err) {
    // log the list error. Note, the stream closes if it encounters an error
  })
  .on('end', function() {
    // end of the stream
  });
```

Since the data is lazily loaded, you can stop the stream and will not incur additional API calls:

```js
var count = 0;

resultStream
  .on('data', function(data) {
    if (++count > 20) {
      // Closing stream, this CAN be called multiple times because the
      // buffer of the queue may already contain additional data
      resultStream.endMarketoStream();
    }
  })
  .on('end', function() {
    // count here CAN be more than 20
    console.log('done, count is', count);
  });
```


## Implemented Marketo Endpoints

### Lead

#### lead.byId(id, options)

Implements [Get Lead by Id](http://developers.marketo.com/documentation/rest/get-lead-by-id/)

param | type | description
------|------|------------
`id`  | int | the lead id to query for
`options` | object | `fields`: a comma separated list or array of fields to retrieve

```js
marketo.lead.byId(3)
  .then(function(data) {
    console.log(data);
  })

// Using the field attribute
marketo.lead.byId(3, ['email', 'lastName'])
  .then(function(data) {
    // data.result[0]
    //
    // {
    //   email: "some@email.com",
    //   lastName: "LastName"
    // }
  });
```


#### lead.find(filterType, filterValues, options)

Implements [Get Multiple Leads by Filter Type](http://developers.marketo.com/documentation/rest/get-multiple-leads-by-filter-type/)

param | type | description
------|------|------------
`filterType`  | string | the field that we will filter on
`filterValues`  | Array<string>/string | the values that we will filter for
`options` | object | `fields`: a comma separated list or array of fields to retrieve
 | | `batchSize`: the number of lead records to be returned (max is 300)
 | | `nextPageToken`: used to paginate through large result sets

```js
marketo.lead.find('email', ['email@one.com', 'email@two.com'])

// or
marketo.lead.find('email', 'email@one.com,email@two.com')
```


#### lead.createOrUpdate(input, options)

Implements [Create/Update Leads](http://developers.marketo.com/documentation/rest/createupdate-leads/)

param | type | description
------|------|------------
`input`  | Array<Object> | An array of lead records to create or update
`options` | object | `action`: one of 4 valid actions (createOnly, updateOnly, ...)
 | | `lookupField`: the field used to dedup on
 | | `partitionName`: not sure what this does yet, :)

```js
// Since the action is not passed in, the default action is 'createOrUpdate'
marketo.lead.createOrUpdate(
    [{'email': 'email@one.com'}, {'email': 'email@two.com'}],
    {lookupField: 'email'}
  )

// The same query without creating new leads
marketo.lead.createOrUpdate(
    [{'email': 'email@one.com'}, {'email': 'email@two.com'}],
    {lookupField: 'email', action: 'updateOnly'}
  )
```

### List

#### list.find(options)

Implements [Get Multiple Lists](http://developers.marketo.com/documentation/rest/get-multiple-lists/)

param | type | description
------|------|------------
`options` | object | `id`: array of ids to filter by
 | | `name`: array of names to filter by
 | | `programName`: array of program names to filter by
 | | `workspaceName`: array of workspaces to filter by

```js
// Retrieve all lists
marketo.list.find()

// Find lists with specific ids
marketo.list.find({id: [1, 2, 3]})

// The same query using CSV instead
marketo.list.find({id: '1,2,3'})

// Name in a specific program
marketo.list.find({
  name: ['some name'],
  workspaceName: ['Default']
})
```


#### list.addLeadsToList(listId, input)

Implements [Add Leads To List](http://developers.marketo.com/documentation/rest/add-leads-to-list/)

param | type | description
------|------|------------
`listId` | int | the id of the list you want to add leads to
`input` | Array<int> | an array of lead ids to be added to the list, not CSV

```js
// Add leads 1, 2, and 3 to list id 1
marketo.list.addLeadsToList(1, [1, 2, 3])


// Same thing, in object form
marketo.list.addLeadsToList(1, [{id: 1}, {id: 2}, {id: 3}])
```

#### list.getLeads(options)

Implements [Get Multiple Leads by List Id](http://developers.marketo.com/documentation/rest/get-multiple-leads-by-list-id)

param | type | description
------|------|------------
`listId` | int | the id of the list you want to get leads
`options` | object | `fields`: a comma separated list or array of fields to retrieve
 | | `batchSize`: the number of lead records to be returned (max is 300)
 | | `nextPageToken`: used to paginate through large result sets
 | | `fields`: a comma separated list or array of fields to retrieve

```js
// Get leads from list id 1
marketo.list.getLeads(1)
```


# Test

### Generating a replay for a test

The initial run of the test has to be run against an actual API, the run (when
in recording mode) should capture the API request/response data, which will then
be used for future calls. What makes this a little tricky is Marketo API
endpoints are unique per account, so we have to convert the captured data to
something else for future purposes. We also need to remove credentials from
these captures as well. Anyway, here's the annoying process of generating data:

##### Running against the actual API

The test looks at 4 environment variables when running, they are:

- `MARKETO_ENDPOINT`
- `MARKETO_IDENTITY`
- `MARKETO_CLIENT_ID`
- `MARKETO_CLIENT_SECRET`

After setting these variables, run `npm run testRecord`.

##### Stripping sensitive information

Run the script `./scripts/strip-fixtures.sh`, which will convert the unique
Marketo host over to `123-abc-456.mktorest.com` in addition to removing the
capture that contains sensitive client id/secret file.

##### Copy the data

The data should now be moved over to `fixtures/123-abc-456.mktorest.com-443`,
preferably with a useful name so it's easier to keep track of in the future.

##### Tip

We are using mocha to run the test, you can run a single test so that you
generate only the needed data. To do so, you append `only` to a test:

```js
    // on a single unit test
    it.only('test description, function() {})

    // or an entire describe
    describe.only('test description, function() {})
```

One more thing to note is that once we've processed the raw data, `node-replay`
will not be able to map it back to the original raw request. This means that if
you run `npm run testRecord`, you will be generating requests against Marketo's
API directly. I highly recommend using `only`.
