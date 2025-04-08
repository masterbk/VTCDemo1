using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo1.Service
{
    public class FirebaseService
    {
        private readonly FirestoreDb _firestore;
        public FirebaseService(FirestoreDb firestoreDb)
        {
            _firestore = firestoreDb;
        }

        public async Task<DocumentSnapshot> SearchAsync(string keyword)
        {
            CollectionReference imagesRef = _firestore.Collection("images");

            Query query = imagesRef.WhereArrayContains("keywords", keyword).Limit(1);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            return snapshot.Documents.FirstOrDefault();
        }
    }
}
