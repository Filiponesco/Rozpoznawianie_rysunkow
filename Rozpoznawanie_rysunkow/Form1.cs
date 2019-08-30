﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeuralNetworkBiblioteka;

namespace Rozpoznawanie_rysunkow
{
    public partial class Form1 : Form
    {
        const int res = 784;
        const int total_data = 1000;
        Kategoria airplanes;
        Kategoria bananas;
        Kategoria cars;
        Kategoria fingers;
        List<OneDraw> trainings;
        List<OneDraw> testings;
        NeuralNetwork nn;

        public Form1()
        {
            InitializeComponent();
            trainings = new List<OneDraw>();
            testings = new List<OneDraw>();
            OpenFiles();
            PreparingData(airplanes);
            PreparingData(bananas);
            PreparingData(cars);
            PreparingData(fingers);

            trainings.AddRange(airplanes.Training);
            trainings.AddRange(bananas.Training);
            trainings.AddRange(cars.Training);
            trainings.AddRange(fingers.Training);

            testings.AddRange(airplanes.Testing);
            testings.AddRange(bananas.Testing);
            testings.AddRange(cars.Testing);
            testings.AddRange(fingers.Testing);

            listBox1.Items.Add("airplanes trening " + airplanes.Training.Count());
            listBox1.Items.Add("wszystkie treningi " + trainings.Count());

            nn = new NeuralNetwork(784, 1, 64, 4);
           // nn = new NeuralNetwork(784, 64, 4);

            for(int i = 0; i < 5; i++)
            {
                TreningEpoki();
                listBox1.Items.Add("Zakonczono nauczanie epoki: " + i+1);
                TestingAll();
            }
        }
        private void OpenFiles()
        {
            airplanes = new Kategoria(Label.airplanes);
            bananas = new Kategoria(Label.bananas);
            cars = new Kategoria(Label.cars);
            fingers = new Kategoria(Label.fingers);
        }
        private Bitmap BytesToBitmap(byte[] data, int inIndex, int height, int width)
        {
            inIndex *= res;
            Bitmap img = new Bitmap(height, width);
            for (int i = 0; i < width; i++)
            {
                for (int k = 0; k < height; k++)
                {
                    byte val = data[inIndex];
                    img.SetPixel(k, i, Color.FromArgb(255 - val, 255 - val, 255 - val));
                    inIndex++;
                }
            }
            return img;
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            int total = 100;
            int x = 0; int y = 0;
            for (int n = 0; n < total; n++)
            {
                Bitmap img = BytesToBitmap(airplanes.Data, n, 28, 28);
                x = (n % 10) * 28;
                y = (n / 10) * 28;
                Graphics g = e.Graphics;
                e.Graphics.DrawImage(img, x, y);
            }
        }
        private byte[] SubArray(byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
        private void PreparingData(Kategoria kat)
        {
            for (int i = 0; i < total_data; i++)
            {
                if (i < total_data * 0.8)
                {
                    kat.Training.Add(new OneDraw(kat.Name, BytesToBitmap(kat.Data, i, 28, 28)));
                }
                else
                {
                    kat.Testing.Add(new OneDraw(kat.Name, BytesToBitmap(kat.Data, i, 28, 28)));
                }
            }
        }
        private void Shuffle(List<OneDraw> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                OneDraw value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        private void TreningEpoki()
        {
            Shuffle(trainings);
            //Trening jednej epoki
            for (int i = 0; i < trainings.Count; i++)
            {
                OneDraw data = trainings[i];
                //listBox1.Items.Add("Size jednej bitmapy:  " + data.Bmp.Size);
                //listBox1.Items.Add("bajty jednego rysunku:  " + data.BrightPixels.Count());
                double[] inputs = data.BrightPixels;
                //normalizacja danych
                //for (int k = 0; k < inputs.Count(); k++)
                //{
                //    inputs[k] /= 255;
                //}
                Label name = trainings[i].Lbl;
                double[] targets = { 0, 0, 0, 0 };
                targets[(int)name] = 1;
                //listBox1.Items.Add("Label: " + name + "Targets: [" + targets[0] + " " + targets[1] + " " + targets[2] + " " + targets[3] + " ]");

                nn.Train(inputs, targets);
            }
        }
        private void TestingAll()
        {
            Shuffle(testings);
            int correct = 0;
            //Trening jednej epoki
            for (int i = 0; i < testings.Count; i++)
            {
                OneDraw data = testings[i];
                double[] inputs = data.BrightPixels;
                Label name = testings[i].Lbl;
                double[] guess = nn.Guess(inputs);
                double maxValue = guess.Max();
                int pom = guess.ToList().IndexOf(maxValue);
                Label guessLbl = Label.airplanes;
                guessLbl += pom;
                listBox1.Items.Add("Label: " + name + "Guess: " + guessLbl);
                //listBox1.Items.Add("Guess: [" + guess[0] + " " + guess[1] + " " + guess[2] + " " + guess[3] + " ]");
                if ((int)guessLbl == (int)data.Lbl)
                    correct++;
            }
            double percent = ((double)correct / (double)testings.Count()) * 100;
            listBox1.Items.Add(" Dokladnosc nauki: " + percent.ToString() + "%");
        }
    }
}
