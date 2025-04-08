using Demo1.Dto.Options;
using Google.Cloud.Firestore;

namespace Demo1.Service
{
    public class FirebaseService
    {
        private readonly FirestoreDb _firestore;
        public FirebaseService(FirestoreDb firestoreDb)
        {
            _firestore = firestoreDb;
        }

        public async Task<WriteResult> CreateAsync<T>(string collectionName, T data)
        {
            var docRef = _firestore.Collection(collectionName).Document();
            var wrireResult = await docRef.SetAsync(data);
            return wrireResult;
        }

        public async Task<DocumentSnapshot?> SearchAsync(string collectionName, string fieldName, string keyword)
        {
            CollectionReference imagesRef = _firestore.Collection(collectionName);

            Query query = imagesRef.WhereArrayContains(fieldName, keyword).Limit(1);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents.FirstOrDefault();
        }
    }
}
