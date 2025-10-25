using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Projects.Scripts.View.PostBox;
using UnityEngine;

namespace Projects.DataManagement.Data.Cloud
{
    public class ServerDatabase
    {
        private static DatabaseReference _dbReference;
        public static List<PostItemData> GetPostItems { get; private set; }
        private static string _id;
        private const string TestId = "";

        public void InitializeAccess(FirebaseApp firebaseApp)
        {
            GetPostItems = new List<PostItemData>();
            _dbReference = FirebaseDatabase.GetInstance(firebaseApp).RootReference;
            Debug.Log("INIT RDB!");
        }

        public static void OnLoadPostData()
        {
            _id = InGameDB.Data.ID;
#if UNITY_EDITOR
            _id = TestId;
#endif
            if (string.IsNullOrEmpty(_id)) return;
            LoadPostBoxData(_id).Forget();
        }

        public static async UniTask LoadPostBoxData(string id)
        {
            DatabaseReference playerReference = _dbReference.Child("rewards").Child(_id);
            if (playerReference == null) return;
            DataSnapshot snapshot = await playerReference.GetValueAsync().AsUniTask().Timeout(TimeSpan.FromSeconds(5));
            if (snapshot == null) throw new Exception();
            await DatabaseSnapshotProcess(snapshot);
        }

        private static async UniTask DatabaseSnapshotProcess(DataSnapshot snapshot)
        {
            GetPostItems.Clear();
            await UniTask.SwitchToMainThread();
            if (snapshot.Exists)
            {
                HashSet<int> existingIds = new HashSet<int>(GetPostItems.Select(item => item.Idx));
                foreach (DataSnapshot data in snapshot.Children)
                {
                    PostItemData newItem = RdbDataParser.ParsePostData(data);
                    if (newItem is not { Consume: false } || existingIds.Contains(newItem.Idx)) continue;
                    GetPostItems.Add(newItem);
                    existingIds.Add(newItem.Idx);
                }
            }
        }
    
        public static async UniTask<bool> Consume(int idx)
        {
            try
            {
                PostItemData item = GetPostItems.FirstOrDefault(i => i.Idx == idx);
                if (item == null) return false;
                DatabaseReference itemRef = _dbReference.Child("rewards").Child(_id).Child(idx.ToString());
                await itemRef.RemoveValueAsync().AsUniTask();
                return GetPostItems.Remove(item);
            }
            catch
            {
                return false;
            }
        }
    }

    public static class ServerDB
    {
        private static readonly ServerDatabase Rdb = new();
        public static Action<FirebaseApp> InitializeAccess => InitDB;
        private static void InitDB(FirebaseApp firebaseApp) => Rdb.InitializeAccess(firebaseApp);
    }
}