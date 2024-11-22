using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace LimpiarCsv
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Habilitar Windows-1252 en .NET Core
            // Agregar opciones al ComboBox
            cmbCsvType.Items.Add("MercadoPago");
            cmbCsvType.Items.Add("Credicoop");
            cmbCsvType.SelectedIndex = 0; // Valor por defecto
        }

        private void ProcessMercadoPagoCSV(string filePath)
        {
            try
            {
                // Configuración para leer el CSV
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ";",                 // Delimitador es la coma
                    HasHeaderRecord = true,           // Se espera una fila de encabezados
                    BadDataFound = null,              // Ignora datos corruptos
                    MissingFieldFound = null          // Ignora campos faltantes
                };

                // Lista de nombres de columnas personalizados
                var newHeaders = new List<string>
                {
                    "EXTERNAL_REFERENCE", "SOURCE_ID", "USER_ID", "PAYMENT_METHOD_TYPE", "PAYMENT_METHOD",
                    "SITE", "TRANSACTION_TYPE", "TRANSACTION_AMOUNT", "TRANSACTION_CURRENCY", "ORIGIN_DATE",
                    "FEE_AMOUNT", "SETTLEMENT_NET_AMOUNT", "SETTLEMENT_CURRENCY", "APPROVAL_DATE", "REAL_AMOUNT",
                    "COUPON_AMOUNT", "METADATA", "ORDER_ID", "SHIPPING_ID", "SHIPMENT_MODE", "PACK_ID",
                    "POI_WALLET_NAME*", "POI_BANK_NAME*"
                };

                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, config))
                {
                    // Leer la primera fila como encabezados
                    csv.Read();
                    csv.ReadHeader();

                    // Verificar que las columnas necesarias existan
                    if (!csv.HeaderRecord.Contains("TRANSACTION_AMOUNT") ||
                        !csv.HeaderRecord.Contains("SOURCE_ID") ||
                        !csv.HeaderRecord.Contains("TRANSACTION_DATE") ||
                        !csv.HeaderRecord.Contains("ORDER_ID"))
                    {
                        MessageBox.Show("El archivo CSV no contiene las columnas necesarias: 'TRANSACTION_AMOUNT', 'SOURCE_ID', 'ORDER_ID'.");
                        return;
                    }

                    // Cargar los registros
                    var records = csv.GetRecords<dynamic>().ToList();

                    // Filtrar y procesar los registros
                    var filteredRecords = records
                        .Where(r =>
                        {
                            // Convertir y filtrar TRANSACTION_AMOUNT
                            string transactionAmountStr = r.TRANSACTION_AMOUNT;

                            // Reemplazar el punto por la coma y convertir a decimal
                            if (decimal.TryParse(transactionAmountStr.Replace(".", ","), NumberStyles.AllowDecimalPoint, new CultureInfo("es-ES"), out decimal transactionAmount))
                            {
                                // Usar el valor con coma como decimal
                                r.TRANSACTION_AMOUNT = transactionAmount.ToString("F2", new CultureInfo("es-ES")); // Mantener el formato con coma
                                if (transactionAmount < 0) return false; // Eliminar si es negativo
                            }
                            else return false; // Eliminar si no es un número válido


                            // Filtrar SOURCE_ID (debe tener 11 dígitos o menos)
                            if (r.SOURCE_ID.Length > 11) return false;

                            // Filtrar ORDER_ID (eliminar si no está vacío)
                            if (!string.IsNullOrEmpty(r.ORDER_ID)) return false;

                            return true; // Si pasa todas las condiciones, se mantiene
                        })
                        .ToList();


                    // Obtener las fechas más antiguas y más recientes de TRANSACTION_DATE
                    var transactionDates = filteredRecords
                        .Select(r => DateTime.Parse(r.TRANSACTION_DATE, null, DateTimeStyles.RoundtripKind))  // Parseo del formato ISO 8601
                        .ToList();

                    DateTime minDate = transactionDates.Min();
                    DateTime maxDate = transactionDates.Max();

                    // Formatear las fechas para el nombre del archivo
                    string formattedMinDate = minDate.ToString("dd 'de' MMMM", new CultureInfo("es-ES"));
                    string formattedMaxDate = maxDate.ToString("dd 'de' MMMM yyyy", new CultureInfo("es-ES"));

                    // Crear el nombre del archivo con las fechas
                    string fileNameWithDates = $"{formattedMinDate} al {formattedMaxDate} MP.csv";





                    // Guardar los registros procesados en un nuevo archivo CSV
                    var outputPath = Path.Combine(Path.GetDirectoryName(filePath), fileNameWithDates);
                    using (var writer = new StreamWriter(outputPath))
                    using (var csvWriter = new CsvWriter(writer, config))
                    {
                        // Escribir los nuevos encabezados
                        foreach (var header in newHeaders)
                        {
                            csvWriter.WriteField(header);
                        }
                        csvWriter.NextRecord();

                        // Escribir los registros filtrados (sin reescribir los encabezados antiguos)
                        foreach (var record in filteredRecords)
                        {
                            csvWriter.WriteRecord(record);
                            csvWriter.NextRecord();
                        }
                    }

                    MessageBox.Show($"Archivo procesado y guardado en {outputPath}");
                }
            }
            catch (HeaderValidationException)
            {
                MessageBox.Show("Error: El archivo CSV no tiene los encabezados esperados.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar el archivo: {ex.Message}");
            }
        }
       
    

        private void ProcessCredicoopCSV(string filePath)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = ",", // Usar coma como delimitador
                    HasHeaderRecord = true, // El archivo tiene encabezado
                    BadDataFound = null,
                    MissingFieldFound = null,
                    HeaderValidated = null // Desactivamos la validación de encabezado
                };

                using (var reader = new StreamReader(filePath))
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Read(); // Leer la primera fila (encabezado)
                    csv.ReadHeader(); // Leer los nombres de las columnas

                    var records = new List<dynamic>();

                    while (csv.Read())
                    {
                        var record = new
                        {
                            Fecha = csv.GetField(0), // Columna 0
                            Concepto = csv.GetField(1), // Columna 1
                            NroCpbte = csv.GetField(2), // Columna 2
                            Debito = csv.GetField(3), // Columna 3
                            Credito = csv.GetField(4), // Columna 4
                            // Ignoramos 'Saldo' (Columna 5)
                            Cod = csv.GetField(6) // Columna 6
                        };

                        // Comprobar si el valor 'Débito' tiene un formato válido
                        var debitoValue = LimpiarYConvertirAEntero(record.Debito);

                        // Si el valor no es válido, continuar con el siguiente registro
                        if (debitoValue > 0) continue; // Saltar registro si 'Débito' es mayor a 0

                        // Comprobar si el valor 'Crédito' tiene un formato válido
                        var creditoValue = LimpiarYConvertirAEntero(record.Credito);



                        // Eliminar registros donde 'Concepto' contiene '30712536051'
                        if (record.Concepto.Contains("30712536051")) continue;
                        if (record.Concepto.Contains("375-0119282")) continue;


                        records.Add(new
                        {
                            record.Fecha,
                            record.Concepto,
                            record.NroCpbte,
                            Debito = debitoValue.ToString("0"), // Solo la parte entera sin decimales
                            Credito = creditoValue.ToString("0"), // Solo la parte entera sin decimales
                            record.Cod
                        });
                    }

                    // Obtener las fechas más antiguas y más recientes de 'Fecha'
                    var transactionDates = records
                        .Select(r => DateTime.ParseExact(r.Fecha, "dd/MM/yyyy", CultureInfo.InvariantCulture))
                        .ToList();

                    DateTime minDate = transactionDates.Min();
                    DateTime maxDate = transactionDates.Max();

                    // Formatear las fechas para el nombre del archivo
                    string formattedMinDate = minDate.ToString("dd 'de' MMMM", new CultureInfo("es-ES"));
                    string formattedMaxDate = maxDate.ToString("dd 'de' MMMM yyyy", new CultureInfo("es-ES"));

                    // Crear el nombre del archivo con las fechas
                    string fileNameWithDates = $"{formattedMinDate} al {formattedMaxDate} Credicoop.csv";

                    // Guardar los registros procesados en un nuevo archivo CSV con el nombre basado en las fechas
                    var outputPath = Path.Combine(Path.GetDirectoryName(filePath), fileNameWithDates);

                    config.Delimiter = ";"; // Cambiamos el delimitador a ';' para la salida

                    using (var writer = new StreamWriter(outputPath, false, Encoding.GetEncoding("Windows-1252"))) // Codificación ANSI (Windows-1252)
                    using (var csvWriter = new CsvWriter(writer, config))
                    {
                        // Escribir los nuevos encabezados manualmente
                        csvWriter.WriteField("Fecha");
                        csvWriter.WriteField("Concepto");
                        csvWriter.WriteField("Nro.Cpbte.");
                        csvWriter.WriteField("Débito");
                        csvWriter.WriteField("Crédito");
                        csvWriter.WriteField("Cód.");
                        csvWriter.NextRecord();

                        // Escribir los registros filtrados excluyendo 'Saldo'
                        foreach (var record in records)
                        {
                            csvWriter.WriteField(record.Fecha);
                            csvWriter.WriteField(record.Concepto);
                            csvWriter.WriteField(record.NroCpbte);
                            csvWriter.WriteField(record.Debito);
                            csvWriter.WriteField(record.Credito);
                            csvWriter.WriteField(record.Cod);
                            csvWriter.NextRecord();
                        }
                    }

                    MessageBox.Show($"Archivo Credicoop procesado y guardado como {fileNameWithDates}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar el archivo Credicoop: {ex.Message}");
            }


        }
        private int LimpiarYConvertirAEntero(string valor)
        {
            // Reemplazar el punto por coma para asegurar que se pueda interpretar correctamente
            valor = valor.Replace(".", ",").Trim();

            // Intentar convertir el valor a decimal primero
            if (decimal.TryParse(valor, NumberStyles.Any, new CultureInfo("es-ES"), out decimal resultadoDecimal))
            {
                // Convertir la parte entera y devolver el valor
                return (int)Math.Floor(resultadoDecimal);
            }

            // Devolver 0 si no se puede convertir correctamente
            return 0;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Verificar la selección en el ComboBox y llamar al método correspondiente
            string selectedCsvType = cmbCsvType.SelectedItem.ToString();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;

                if (selectedCsvType == "MercadoPago")
                {
                    ProcessMercadoPagoCSV(filePath);
                }
                else if (selectedCsvType == "Credicoop")
                {
                    // ProcessCredicoopCSV(filePath);
                    ProcessCredicoopCSV(filePath);
                }
            }
        }
    }
}
