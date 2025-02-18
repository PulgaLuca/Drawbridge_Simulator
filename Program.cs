/*
 * Autore: Pulga Luca
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Media;

namespace Drawbridge_Simulator
{
    class Program
    {
        #region CDC.
        const int NUM_AUTO_SUL_PONTE = 4; // Portata max del ponte in numero di auto.
        const int MAX_X_SENSO = 7; // Numero max di auto consecutive in un senso di marcia.

        static object _lock = new object(); // Lock per la sezione critica.

        static List<string> parcheggioAutoDX = new List<string>(); // Auto a destra.
        static List<string> parcheggioAutoSX = new List<string>(); // Auto a sinistra.

        static SemaphoreSlim semaphore = new SemaphoreSlim(NUM_AUTO_SUL_PONTE); // Determina quante auto possono stare sul ponte contemporaneamente.

        static Thread t = new Thread(TitleBar); // Animazione title bar.
        static Thread ship = new Thread(Ship); // Animazione barca.

        static int conDx = 0; // Numero auto a destra.
        static int contSx = 0; // Numero auto a sinistra.

        static bool inTransito = false; // Controlla se stanno passando sul ponte delle auto.
        #endregion

        /// <summary>
        /// Avvio della simulazione.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            t.Start();
            char opz = ' ';
            Console.CursorVisible = false;
            Console.SetWindowSize(120, 46); // Dimensione finestra all'avvio.

            Background(); // Setta lo sfondo.

            while (true)
            {
                Menu(out opz); // Richiede l'opzione.

                switch (opz) // Esegue l'opzione scelta.
                {
                    #region Aggiunge auto a sinistra.
                    case 'L':
                        NuovaAutoSx(); // Aggiunge in coda di attesa un'auto a sinistra.
                        break;
                    #endregion

                    #region Aggiunge auto a destra.
                    case 'R':
                        NuovaAutoDx(); // Aggiunge in coda di attesa un'auto a destra.
                        break;
                    #endregion

                    #region Passaggio auto.
                    case 'P':
                        PassaggioSulPonte(); // Avvia la simualazione.
                        break;
                    #endregion

                    #region Passaggio nave.
                    case 'S':
                        if (inTransito == false && ship.ThreadState == ThreadState.Background)
                            ship.Start(); // Passaggio della barca.
                        else if (inTransito == false)
                        {
                            ship = new Thread(Ship);
                            ship.Start();
                        }
                        else
                        {
                            Console.CursorLeft = 50;
                            Console.CursorTop = 15;
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.SetCursorPosition(34, 13);
                            Console.WriteLine("-----------------------------------------------------");
                            string mex = "AUTO IN TRANSITO: LA NAVE PARTIRA' APPENA FINITO IL TRANSITO";
                            Console.SetCursorPosition(34, 17);
                            Console.WriteLine("-----------------------------------------------------");
                            Console.SetCursorPosition(28, 15);
                            Console.WriteLine(mex);
                        }
                        break;
                    #endregion

                    #region Uscita app.
                    case 'E':
                        Environment.Exit(-1); // Chiusura app.
                        break;
                        #endregion
                }
            }
        }

        #region Aggiunge auto al parcheggio SX.
        /// <summary>
        /// Aggiunge un'auto nel parcheggio a sinistra.
        /// </summary>
        static void NuovaAutoSx()
        {
            string autoSX = "AutoSX" + contSx++;
            parcheggioAutoSX.Add(autoSX); // Aggiunge una auto in attesa a destra.

            StampaAutoSinistra();
            StampaAutoDestra();
        }
        #endregion

        #region Aggiunge auto al parcheggio DX.
        /// <summary>
        /// Aggiunge un'auto nel parcheggio a sinistra.
        /// </summary>
        static void NuovaAutoDx()
        {
            string autoDX = "AutoDX" + conDx++;
            parcheggioAutoDX.Add(autoDX); // Aggiunge una auto in attesa a destra.

            StampaAutoDestra();
            StampaAutoSinistra();
        }
        #endregion

        #region Stampa auto sinistra.
        /// <summary>
        /// Stampa e aggiorna la auto in attesa a sinistra.
        /// </summary>
        static void StampaAutoSinistra()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            // Elenca le auto di sinistra.
            for (int i = 0; i < parcheggioAutoSX.Count; i++)
            {
                Console.SetCursorPosition(2, 5 + i);
                Console.WriteLine(parcheggioAutoSX[i]);
            }
        }
        #endregion

        #region Stampa auto destra.
        /// <summary>
        /// Stampa e aggiorna la auto in attesa a destra.
        /// </summary>
        static void StampaAutoDestra()
        {
            // Elenca le auto di destra.
            Console.ForegroundColor = ConsoleColor.Green;
            for (int i = 0; i < parcheggioAutoDX.Count; i++)
            {
                Console.SetCursorPosition(105, 5 + i);
                Console.WriteLine(parcheggioAutoDX[i]);
            }
        }
        #endregion

        #region Attraversamento verso sinistra.
        /// <summary>
        /// Attraversamento della auto di destra verso sinistra.
        /// </summary>
        /// <param name="auto">Targa dell'auto che transita.</param>
        /// <param name="riga">Corsia dell'auto.</param>
        /// <returns></returns>
        static async Task AttraversaVersoSx(string targaAuto, int corsiaAuto)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            semaphore.Wait(); // Solo il numero specificato da NUM_AUTO_SUL_PONTE ovver ortata max del ponte in numero di auto, può entrare.
            for (int i = 0; i < 60; i++)
            {
                lock (_lock) // Solo un'auto alla volta entra nella sezione critica.
                {
                    Console.CursorTop = 25 + corsiaAuto;
                    Console.CursorLeft = 85 - i;
                    Console.Write(targaAuto);
                    Console.CursorLeft = 84 - i + targaAuto.Length;
                    Console.Write(" ");
                    Console.CursorLeft = 84 - i;
                    Console.Write(targaAuto);
                }
                await Task.Delay(80); // Aggiornamento animazione attraversamento auto.
            }
            semaphore.Release(); // Uscita auto dal ponte.
        }
        #endregion

        #region Attraversamento verso sinistra.
        /// <summary>
        /// Attraversamento della auto di sinistra verso destra.
        /// </summary>
        /// <param name="auto">Targa dell'auto che transita.</param>
        /// <param name="riga">Corsia dell'auto.</param>
        /// <returns></returns>
        static async Task AttraversaVersoDx(string targaAuto, int corsiaAuto)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            semaphore.Wait(); // Solo il numero specificato da NUM_AUTO_SUL_PONTE ovver ortata max del ponte in numero di auto, può entrare.
            for (int i = 0; i < 60; i++)
            {
                lock (_lock) // Solo un'auto alla volta entra nella sezione critica.
                {
                    Console.CursorTop = 25 + corsiaAuto;
                    Console.CursorLeft = 25 + i;
                    Console.Write(targaAuto);
                    Console.CursorLeft = 25 + i;
                    Console.Write("  ");
                    Console.Write(targaAuto);
                }
                await Task.Delay(80); // Aggiornamento animazione attraversamento auto.
            }
            semaphore.Release();  // Uscita auto dal ponte.
        }
        #endregion

        #region Transito delle auto in senso unico alternato.
        /// <summary>
        /// Transito delle auto in senso unico alternato.
        /// </summary>
        static void PassaggioSulPonte()
        {
            List<string> autoInTransito = new List<string>();
            while ((parcheggioAutoSX.Count > 0 || parcheggioAutoDX.Count > 0) && !(ship.ThreadState == ThreadState.Running)) // Sono ancora presenti auto?
            {
                inTransito = true;
                if (parcheggioAutoSX.Count > 0)
                {
                    autoInTransito.Clear();
                    for (int i = 0; i < MAX_X_SENSO && parcheggioAutoSX.Count > 0; i++) // Massimo 7 auto del parcheggio di sinistra.
                    {
                        autoInTransito.Add(parcheggioAutoSX[parcheggioAutoSX.Count - 1]);
                        parcheggioAutoSX.RemoveAt(parcheggioAutoSX.Count - 1); // Tolgo le auto che passano.
                    }

                    Console.Clear();
                    Background();
                    StampaAutoSinistra();
                    StampaAutoDestra();

                    List<Task> attraversamentiAutoSinistra = new List<Task>();

                    for (int i = 0; i < autoInTransito.Count; i++)
                    {
                        attraversamentiAutoSinistra.Add(AttraversaVersoDx(autoInTransito[i], i % NUM_AUTO_SUL_PONTE)); // Set dei task (auto) che passano sulle varie corsie.
                    }
                    Task.WaitAll(attraversamentiAutoSinistra.ToArray()); // Tutti gli altri task aspettano il termine del passaggio delle altre auto.
                }

                if (parcheggioAutoDX.Count > 0)
                {
                    autoInTransito.Clear();
                    for (int i = 0; i < MAX_X_SENSO && parcheggioAutoDX.Count > 0; i++) // Massimo 7 auto del parcheggio di destra.
                    {
                        autoInTransito.Add(parcheggioAutoDX[parcheggioAutoDX.Count - 1]);
                        parcheggioAutoDX.RemoveAt(parcheggioAutoDX.Count - 1); // Tolgo le auto che passano.
                    }

                    Console.Clear();
                    Background();
                    StampaAutoSinistra();
                    StampaAutoDestra();

                    List<Task> attraversamentiAutoDestra = new List<Task>();

                    for (int i = 0; i < autoInTransito.Count; i++)
                    {
                        attraversamentiAutoDestra.Add(AttraversaVersoSx(autoInTransito[i], i % NUM_AUTO_SUL_PONTE)); // Set dei task (auto) che passano sulle varie corsie.
                    }
                    Task.WaitAll(attraversamentiAutoDestra.ToArray()); // Tutti gli altri task aspettano il termine del passaggio delle altre auto.
                }
            }
            inTransito = false;
        }
        #endregion

        #region Background.
        /// <summary>
        /// Set del background con il fiume e il ponte.
        /// </summary>
        static void Background()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.CursorTop = 1;
            Console.CursorLeft = 25;
            Console.WriteLine("SIMULAZIONE ATTRAVERSAMENTO PONTE LEVATOI SENSO UNICO ALTERNATO.");

            Console.CursorTop = 2;
            Console.CursorLeft = 2;
            Console.WriteLine(" Add [L]eft"); // Aggiunge auto a sinistra in coda.

            Console.CursorTop = 2;
            Console.CursorLeft = 105;
            Console.WriteLine(" Add [R]ight"); // Aggiunge auto a destra in coda.

            Console.CursorTop = 2;
            Console.CursorLeft = 45;
            Console.WriteLine(" Car [P]assage"); // Avvia la simulazione.

            Console.CursorTop = 2;
            Console.CursorLeft = 20;
            Console.WriteLine(" [S]hip passage"); // Chiude il ponte per il passaggio di una nave.

            Console.CursorTop = 2;
            Console.CursorLeft = 70;
            Console.WriteLine(" [E]xit from drawbridge\n"); // Esce dal programma.
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Cyan;
            for (int i = 4; i < 24; i++)
            {
                Console.SetCursorPosition(42, i);
                Console.Write("░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(35, 24);
            Console.Write("═════════════════════════════════════════════════");
            Console.SetCursorPosition(35, 25 + NUM_AUTO_SUL_PONTE); // Set della distanza fra le due sponde del ponte.
            Console.Write("═════════════════════════════════════════════════");
            Console.ForegroundColor = ConsoleColor.Cyan;
            int startY = 26 + NUM_AUTO_SUL_PONTE;
            for (int i = startY; i < Math.Min(47, Console.WindowHeight); i++)
            {
                Console.SetCursorPosition(42, i);
                Console.Write("░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░");
            }

        }
        #endregion

        #region Title bar.
        /// <summary>
        /// Animazione title bar.
        /// </summary>
        static void TitleBar()
        {
            string progressbar = "Ponte senso unico alternato - Pulga Luca";
            var title = "";
            while (true)
            {
                for (int i = 0; i < progressbar.Length; i++)
                {
                    title += progressbar[i];
                    Console.Title = title;
                    Thread.Sleep(100);
                }
                title = "";
            }
        }
        #endregion

        #region Ship.
        /// <summary>
        /// Animazione passaggio barca.
        /// </summary>
        static void Ship()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.CursorTop = 1;
            Console.CursorLeft = 25;
            Console.WriteLine("SIMULAZIONE ATTRAVERSAMENTO PONTE LEVATOI SENSO UNICO ALTERNATO.");

            Console.CursorTop = 2;
            Console.CursorLeft = 2;
            Console.WriteLine(" Add [L]eft"); // Aggiunge auto a sinistra in coda.

            Console.CursorTop = 2;
            Console.CursorLeft = 105;
            Console.WriteLine(" Add [R]ight"); // Aggiunge auto a destra in coda.

            Console.CursorTop = 2;
            Console.CursorLeft = 45;
            Console.WriteLine(" Car [P]assage"); // Avvia la simulazione.

            Console.CursorTop = 2;
            Console.CursorLeft = 20;
            Console.WriteLine(" [S]hip passage"); // Chiude il ponte per il passaggio di una nave.

            Console.CursorTop = 2;
            Console.CursorLeft = 70;
            Console.WriteLine(" [E]xit from drawbridge\n"); // Esce dal programma.
            Console.ResetColor();
            for (int i = 4; i < 50; i++)
            {
                Console.SetCursorPosition(35, i);
                Console.Write("~'^~'^~'^~'^~'^~'^~~'^~'^~'^~'^~'^~'^~~'^~'^~'^~'^~");
            }
            Thread.Sleep(1000);

            for (int j = 0; j < 30; j++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.Clear();
                for (int i = 4; i < 50; i++)
                {
                    Console.SetCursorPosition(35, i);
                    Console.Write("~'^~'^~'^~'^~'^~'^~~'^~'^~'^~'^~'^~'^~~'^~'^~'^~'^~");
                }
                Console.SetCursorPosition(45, 10 + j);
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.WriteLine("       _    _o");
                Console.SetCursorPosition(45, 11 + j);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("  __|_|__|_|__");
                Console.SetCursorPosition(45, 12 + j);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" |____________|__");
                Console.SetCursorPosition(45, 13 + j);
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(" |o o o o o o o o /");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.SetCursorPosition(45, 14 + j);
                Console.WriteLine(" ~'`~'`~'`~'`~'`~'`~'`~");
                Thread.Sleep(150);
            }

            Console.Clear();
            Console.CursorVisible = false;
            Background();
            StampaAutoSinistra();
            StampaAutoDestra();
        }
        #endregion

        #region Menù.
        /// <summary>
        /// Visualizza il menù e richiede l'opzione.
        /// </summary>
        /// <returns>Scelta dell'utente.</returns>
        static void Menu(out char ch)
        {
            do // Legge e controlla l'opzione scelta.
            {
                ch = Console.ReadKey(true).KeyChar;
                ch = char.ToUpper(ch);
            }
            while (!((ch == 'L') || (ch == 'R') || (ch == 'S') || (ch == 'E') || (ch == 'P'))); // Controllo scelta.
        }
        #endregion

    }
}
