// Run from repo root. Uses canonical Extended JSON to preserve ObjectId/Int64/Date.
const fs = require('node:fs');
const path = require('node:path');
const name = process.env.MONGO_DB || 'ecommerce_dev';
if (!/^[A-Za-z][A-Za-z0-9_]*_(dev|test|bench)$/.test(name)) throw new Error('Use a _dev, _test or _bench database.');
const connection = db.getMongo();
const target = connection.getDB(name);
const collections = ['danhmuc','sanpham','hoadon'];
const dataset = Object.fromEntries(collections.map(c => [c, EJSON.parse(fs.readFileSync(path.join('data',c+'.json'),'utf8'), {relaxed:false})]));
function sorted(value) {
  if (Array.isArray(value)) return value.map(sorted);
  if (value && typeof value === 'object') return Object.fromEntries(Object.keys(value).sort().map(k => [k,sorted(value[k])]));
  return value;
}
const canonical = value => JSON.stringify(sorted(EJSON.serialize(value,{relaxed:false})));
const existing = Object.fromEntries(collections.map(c => [c,target.getCollection(c).find().sort({_id:1}).toArray()]));
if (collections.some(c => existing[c].length > 0)) {
  if (!collections.every(c => canonical(existing[c]) === canonical([...dataset[c]].sort((a,b)=>a._id.toString().localeCompare(b._id.toString())))))
    throw new Error('Database contains other or modified data. No overwrite; choose a new empty development/test database.');
  print('NO-OP: identical seed dataset already exists.');
} else {
  const hello = target.runCommand({hello:1});
  if (!hello.setName && hello.msg !== 'isdbgrid') throw new Error('Seed requires a replica set or sharded cluster for transactions.');
  const validators = JSON.parse(fs.readFileSync('database/validators.json','utf8'));
  for (const c of collections) {
    if (!target.getCollectionNames().includes(c)) target.createCollection(c,{validator:validators[c]});
    else {
      const result = target.runCommand({collMod:c,validator:validators[c]});
      if (result.ok !== 1) throw new Error('Could not apply validator: '+c);
    }
  }
  load('database/indexes.mongosh.js');
  const session = connection.startSession();
  try {
    session.startTransaction();
    const sessionDb = session.getDatabase(name);
    for (const c of collections) {
      if (sessionDb.getCollection(c).countDocuments({}) !== 0) throw new Error('Database changed during seed; aborting.');
      sessionDb.getCollection(c).insertMany(dataset[c]);
    }
    session.commitTransaction();
  } catch (error) {
    try { session.abortTransaction(); } catch { /* Preserve the original failure. */ }
    throw error;
  } finally { session.endSession(); }
  print('Seed completed: ' + name);
}
for (const c of collections) print(c+': '+target.getCollection(c).countDocuments({}));
