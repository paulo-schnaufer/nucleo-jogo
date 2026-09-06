// NÚCLEO: Última Onda — Ranking persistente (PlayerPrefs)
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Nucleo
{
    [Serializable]
    public class HighScoreEntry
    {
        public string initials;
        public int score;
    }

    [Serializable]
    internal class HighScoreList
    {
        public List<HighScoreEntry> entries = new List<HighScoreEntry>();
    }

    /// <summary>
    /// Ranking local persistido via PlayerPrefs (sobrevive entre execuções do
    /// .exe, sem precisar de arquivo próprio ou servidor). JsonUtility não
    /// serializa uma List no nível raiz, por isso o wrapper HighScoreList.
    /// </summary>
    public static class HighScoreManager
    {
        private const string PrefsKey = "NucleoHighScores";
        private const int MaxEntries = 10;

        public static List<HighScoreEntry> LoadScores()
        {
            string json = PlayerPrefs.GetString(PrefsKey, "");
            if (string.IsNullOrEmpty(json))
                return new List<HighScoreEntry>();

            HighScoreList list = JsonUtility.FromJson<HighScoreList>(json);
            return list?.entries ?? new List<HighScoreEntry>();
        }

        /// <summary>True se essa pontuação entraria no top 10 atual.</summary>
        public static bool QualifiesForTopScores(int score)
        {
            var scores = LoadScores();
            if (scores.Count < MaxEntries) return true;
            return score > scores.Min(e => e.score);
        }

        private static int _lastSavedRank = -1;

        public static int SaveScore(string initials, int score)
        {
            var scores = LoadScores();

            var newEntry = new HighScoreEntry
            {
                initials = string.IsNullOrWhiteSpace(initials) ? "???" : initials.ToUpperInvariant(),
                score = score
            };
            scores.Add(newEntry);

            scores = scores.OrderByDescending(e => e.score).ToList();
            int rank = scores.IndexOf(newEntry) + 1; // 1-based, calculado ANTES do corte pro Top 10

            scores = scores.Take(MaxEntries).ToList();

            var wrapper = new HighScoreList { entries = scores };
            PlayerPrefs.SetString(PrefsKey, JsonUtility.ToJson(wrapper));
            PlayerPrefs.Save();

            _lastSavedRank = rank <= MaxEntries ? rank : -1;
            return _lastSavedRank;
        }

        /// <summary>
        /// Chamado uma vez pelo NarrativeUIController ao montar o leaderboard, pra saber
        /// qual linha (se houver) acabou de ser salva nesta sessão. Consome o valor
        /// (volta a -1) pra não ficar destacando pra sempre em visitas futuras ao ranking.
        /// </summary>
        public static int ConsumeLastSavedRank()
        {
            int r = _lastSavedRank;
            _lastSavedRank = -1;
            return r;
        }

        /// <summary>Útil em testes — apaga o ranking salvo.</summary>
        public static void ClearScores()
        {
            PlayerPrefs.DeleteKey(PrefsKey);
        }
    }
}
