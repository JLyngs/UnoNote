using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace UnoNote
{
    public partial class MainWindow : Window
    {
        private string[] csvFiles;
        private readonly Dictionary<string, string> notesDictionary = new Dictionary<string, string>();

        public MainWindow()
        {
            InitializeComponent();
            csvFiles = Directory.GetFiles("../../../NotesCSV");
            LoadNotesFromCSV();
            DisplayNotes();

        }


        private void LoadNotesFromCSV()
        {
            foreach (string file in csvFiles)
            {
                string NoteName = Path.GetFileNameWithoutExtension(file);
                string NoteContent = File.ReadAllText(file);
                notesDictionary.Add(NoteName, NoteContent);
            }
        }

        private void DisplayNotes()
        {
            foreach (string noteName in notesDictionary.Keys)
            {
                NoteNameListBox.Items.Add(noteName);
            }
        }

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void ButtonMinimize(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ButtonExit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonMaximize(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void NoteTitle_TextChanged(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && NoteNameListBox.SelectedItem != null)
            {
                string oldNoteName = NoteNameListBox.SelectedItem.ToString();
                string newNoteName = NoteTitle.Text;

                if (!string.IsNullOrEmpty(newNoteName) && !notesDictionary.ContainsKey(newNoteName))
                {

                    string noteContent = notesDictionary[oldNoteName];
                    notesDictionary.Remove(oldNoteName);
                    notesDictionary.Add(newNoteName, noteContent);


                    int selectedIndex = NoteNameListBox.SelectedIndex;
                    NoteNameListBox.Items[selectedIndex] = newNoteName;

                    NoteNameListBox.SelectedItem = newNoteName;


                    string oldFilePath = Path.Combine("../../../NotesCSV", oldNoteName + ".csv");
                    string newFilePath = Path.Combine("../../../NotesCSV", newNoteName + ".csv");
                    if (File.Exists(oldFilePath))
                    {
                        File.Move(oldFilePath, newFilePath);
                    }

                    SaveNotesToCSV();
                }
            }
        }

        private static bool IsImageFile(string filename)
        {
            string ext = Path.GetExtension(filename).ToLower();
            return ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".bmp";
        }

        private void NoteEditorTextBox_PreviewDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    if (NoteEditorTextBox.Document.PasteImageFiles(NoteEditorTextBox.Selection, files.Where(IsImageFile)))
                        e.Handled = true;
                }
            }
        }

        private void NoteEditorTextBox_PreviewDragEnter(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length > 0 && files.Where(IsImageFile).Any())
            {
                e.Handled = true;
            }
        }

        private void DeleteNote(object sender, RoutedEventArgs e)
        {
            if (NoteNameListBox.SelectedItem != null)
            {
                string selectedNoteName = NoteNameListBox.SelectedItem.ToString();

                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this note?", selectedNoteName, MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    notesDictionary.Remove(selectedNoteName);
                    NoteNameListBox.Items.Remove(selectedNoteName);

                    string filePath = Path.Combine("../../../NotesCSV", selectedNoteName + ".csv");
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    SaveNotesToCSV();
                }
            }
        }

        private void RenameNote(object sender, RoutedEventArgs e)
        {
            if (NoteNameListBox.SelectedItem != null)
            {
                string selectedNoteName = NoteNameListBox.SelectedItem.ToString();
                string newNoteName = Microsoft.VisualBasic.Interaction.InputBox("Enter new name for the note", "Rename Note", selectedNoteName);
                if (newNoteName != "")
                {
                    // Update the dictionary
                    string noteContent = notesDictionary[selectedNoteName];
                    notesDictionary.Remove(selectedNoteName);
                    notesDictionary.Add(newNoteName, noteContent);

                    // Update the ListBox
                    int selectedIndex = NoteNameListBox.SelectedIndex;
                    NoteNameListBox.Items[selectedIndex] = newNoteName;

                    NoteNameListBox.SelectedItem = newNoteName;

                    string filePath = Path.Combine("../../../NotesCSV", selectedNoteName + ".csv");
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    SaveNotesToCSV();
                }
            }
        }

        private void ButtonAddNewNote(object sender, RoutedEventArgs e)
        {
            string newNoteName;
            int newNoteCounter = 1;
            do
            {
                newNoteName = "New Note" + newNoteCounter++;
            } while (notesDictionary.ContainsKey(newNoteName));

            notesDictionary.Add(newNoteName, "");
            NoteNameListBox.Items.Add(newNoteName);
            NoteNameListBox.SelectedItem = newNoteName;
            NoteEditorTextBox.Text = "";
            SaveNotesToCSV();
        }

        private void ListBoxItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem item)
            {
                item.IsSelected = true;
                ContextMenu cm = FindResource("ListBoxItemContextMenu") as ContextMenu;
                cm.PlacementTarget = item;
                cm.IsOpen = true;
            }
        }

        private void SaveNotesToCSV()
        {
            string directoryPath = "../../../NotesCSV";
            foreach (var entry in notesDictionary)
            {
                string filePath = Path.Combine(directoryPath, entry.Key + ".csv");
                File.WriteAllText(filePath, entry.Value);
            }
        }

        private void NoteNameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NoteNameListBox.SelectedItem != null)
            {
                string? selectedNoteName = NoteNameListBox.SelectedItem?.ToString();
                NoteEditorTextBox.Text = notesDictionary[selectedNoteName];
            }
        }

        private void NoteEditorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (NoteNameListBox.SelectedItem != null)
            {
                string selectedNoteName = NoteNameListBox.SelectedItem.ToString();
                notesDictionary[selectedNoteName] = NoteEditorTextBox.Text;
                SaveNotesToCSV();
            }
        }
    }
}
