using Demo1.Dto.Options;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;

namespace Demo1.Service
{
    public class FirebaseService
    {
        private readonly FirestoreDb _firestore;
        private readonly ILogger<FirebaseService> _logger;
        public FirebaseService(FirestoreDb firestoreDb, ILogger<FirebaseService> logger)
        {
            _firestore = firestoreDb;
            _logger = logger;
        }

        public async Task<WriteResult?> CreateAsync<T>(string? collectionName, T data)
        {
            try
            {
                var docRef = _firestore.Collection(collectionName ?? "images").Document();
                var wrireResult = await docRef.SetAsync(data);
                return wrireResult;
            }
            catch(Exception ex)
            {
                _logger.LogError($"[{nameof(FirebaseService)}.{nameof(CreateAsync)}] => Error create document: {ex.Message}");
                return null;
            }
        }

        public async Task<DocumentSnapshot?> SearchAsync(string? collectionName, string fieldName, string keyword)
        {
            CollectionReference imagesRef = _firestore.Collection(collectionName??"images");

            Query query = imagesRef.WhereArrayContains(fieldName, keyword).Limit(1);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents.FirstOrDefault();
        }
    }
}
