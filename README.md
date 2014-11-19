# node-marketo

This implements (a subset of) Marketo's REST API.

## Overview

This library is a simple wrapper around Marketo's REST API and does not aim to be anything more than that.

## Creating a connection

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
  .spread(function(data, resp) {
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


## Implemented API


### Lead

#### lead.byId(id, options)

Implements [Get Lead by Id](http://developers.marketo.com/documentation/rest/get-lead-by-id/)

param | type | description
------|------|------------
`id`  | int | the lead id to query for
`options` | object | `fields`: a comma separated list or array of fields to retrieve

```js
marketo.lead.byId(3)
  .spread(function(data) {
    console.log(data);
  })

// Using the field attribute
marketo.lead.byId(3, ['email', 'lastName'])
  .spread(function(data) {
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
marketo.lead.addLeadsToList(1, [1, 2, 3])


// Same thing, in object form
marketo.lead.addLeadsToList(1, [{id: 1}, {id: 2}, {id: 3}])
```
