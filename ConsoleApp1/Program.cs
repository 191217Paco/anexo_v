using PLADSE.Servicios.API.RENAPO;
using PLADSE.Servicios.API.RENAPO.serviciosdeconsulta;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsultaCURP = PLADSE.Servicios.API.RENAPO.ConsultaCURP;

namespace _2act
{
    class Program
    {
        static void Main(string[] args)
        {
            string queryHccNt = "SELECT nt.idnomina, HCC.num_cheque as no_comprobante, ur = 'R07',HCC.qna_proc, HCC.rfc, HCC.t_nomina, ((((right('00' + CONVERT([varchar](2), cod_pago, (0)), (2)) " +
                                        "+ right('00' + CONVERT([varchar](2), unidad, (0)), (2))) +right('00' + CONVERT([varchar](2), SubUnidad, (0)), (2))) " +
                                        "+right('' + CONVERT([varchar](7), replace(cat_Puesto, ' ', ''), (0)),(7))) +right('000' + format(horas * 10, 'f0'), (3))) " +
                                        "+right('000000' + CONVERT([varchar](6), cons_Plaza, (0)),(6)) as plaza, HCC.qna_pago as fecha_pago , " +
                                        "HCC.qna_ini as fecha_inicio, HCC.qna_fin as fecha_termino, HCC.tot_perc_cheque as percepciones, " +
                                        "HCC.tot_ded_cheque as deducciones, tot_neto_cheque as neto, " +
                                        "(((right('00' + CONVERT([varchar](2), HCC.ent_fed, (0)), (2)) + CONVERT([varchar](4), HCC.ct_clasif, (0))) + CONVERT([varchar](4)," +
                                        "HCC.ct_id,(0)))+right('0000' + CONVERT([varchar](4), HCC.ct_secuencial, (0)),(4)))+HCC.ct_digito_ver as 'cct', " +
                                        "HCC.tipo_pago as forma_pago, cve_banco = '', HCC.mot_mov as motivo, nivel_cm = 'I', HCC.qna_proc, HCC.num_cheque, HCC.cheque_dv, " +
                                        "grupo = 'OT' + (right('00' + CONVERT([varchar](2), HCC.cons_qna_proc, (0)), (2))) , HCC.cons_qna_proc, * " +
                                        "FROM hist_cheque_cpto_c0 HCC left join nominas_timbrado nt " +
                                        "on(HCC.qna_proc = nt.qna_proc) and(HCC.cons_qna_proc = nt.cons_qna_proc)";


            List<string> listQuery = new List<string>() { queryHccNt };

            AcoplamientoSentencias(listQuery);
            Console.Read();


        }

       

        public static void AcoplamientoSentencias(List<string> listquery)
        {
            DataTable dtHcc = new DataTable();
            DataTable dtE = new DataTable();
            DataTable dtEC = new DataTable();
            DataTable dtEnss = new DataTable();
            DataTable dtEec = new DataTable();

            dtHcc = EjecutarSentencia(listquery[0]);
            

            foreach (DataRow rowHcc in dtHcc.Rows)
            {
                Console.WriteLine("----------------------------------------------------------");
                Console.WriteLine(rowHcc[0].ToString() + " " + rowHcc[1].ToString() + " " + rowHcc["rfc"].ToString() + " " + rowHcc[3].ToString() + " " + rowHcc[4].ToString() + " fecha pago " + rowHcc["qna_proc"].ToString());
                string queryRfcs_E = "select e.rfc, e.nombre as nombres, e.paterno as primer_apellido, e.materno as segundo_apellido " +
                                     "from empleado e " +
                                     "where '" + rowHcc["rfc"] + "' = e.rfc";
        
                dtE = EjecutarSentencia(queryRfcs_E);
                if (dtE.Rows.Count == 0)
                {
                    string queryRfcs_Ps = "select distinct ps.rfc_sust, ps.nombre_sust as nombres, ps.paterno_sust as primer_apellido, ps.materno_sust as segundo_apellido from pagos_sustitutos ps where '"+ rowHcc["rfc"] + "' = ps.rfc_sust    ";
                    dtE = EjecutarSentencia(queryRfcs_Ps);
                    
                }
                Console.WriteLine("Datos de empleado");
                foreach (DataRow rowE in dtE.Rows) { Console.WriteLine(rowE[0].ToString() + " " + rowE[1].ToString() + " " + rowE[2].ToString() + " " + rowE[3].ToString()); }

                if (dtE.Rows.Count >= 1)
                {
                    var tupla = FindCurp(dtEC, rowHcc);

                    dtEC = tupla.dtEC;
                    rowHcc["rfc"] = tupla.rowHcc["rfc"];
                    Console.WriteLine("Que sea lo que dios quiera XD " + rowHcc["rfc"].ToString());
                    Console.WriteLine("que paso aca " + dtEC.Rows.Count);
                    Boolean result = CurpValida(dtEC.Rows[0]["curp"].ToString());

                    if (!result)
                    {
                        
                        string queryInsertInvalidCurp = "IF NOT EXISTS (SELECT * FROM EXCEPCIONES_RFC_CURP WHERE rfc = '"+ dtEC.Rows[0]["rfc"] + "') BEGIN " +
                            "INSERT INTO [dbo].[EXCEPCIONES_RFC_CURP]([idexcepcion],[RFC],[CURP],[nombre],[qna_proc],[cons_qna_proc]) " +
                            "VALUES( newid(),'" + dtEC.Rows[0]["rfc"] + "','" + dtEC.Rows[0]["curp"] + "','" + dtE.Rows[0]["nombres"] + "'," + dtHcc.Rows[0]["qna_proc"] + "," + dtHcc.Rows[0]["cons_qna_proc"] + ") END";
                        EjecutarSentencia(queryInsertInvalidCurp);
                    }
                    Console.WriteLine("Datos de empleados curps");
                    foreach (DataRow rowEc in dtEC.Rows) { Console.WriteLine(rowEc[0].ToString() + " " + rowEc[1].ToString()); }

                    string queryRfcs_Enss = "select enss.rfc, enss.numero_nss as nss from empleado_nss as enss where '" + rowHcc["rfc"] + "' = enss.rfc";
                    dtEnss = EjecutarSentencia(queryRfcs_Enss);

                    if (dtEnss.Rows.Count > 0)
                    {
                        foreach (DataRow rowEnss in dtEnss.Rows)
                        {
                            Console.WriteLine("Datos de seguro social");
                            if (rowEnss["nss"].ToString().Length == 11)
                            {
                                Console.WriteLine("tas mal mijooooooooooo");
                                rowEnss[1] = "";
                            }
                            Console.WriteLine(rowEnss[0].ToString() + " " + rowEnss[1].ToString());
                        }
                    }
                    else
                    {
                        dtEnss.Columns.Add(new DataColumn("rfc")); rowHcc["rfc"].ToString();
                        dtEnss.Columns.Add(new DataColumn("nss"));
                        DataRow dtr = dtEnss.NewRow();
                        dtr["rfc"] = rowHcc["rfc"];
                        dtr["nss"] = "";
                        dtEnss.Rows.Add(dtr);

                    }


                    Console.WriteLine("----------------------------------------------------------");
                    string periodo = AcomodaPeriodo(rowHcc["qna_proc"].ToString());

                    /*
                    string queryAnexo_V = "INSERT INTO [dbo].[anexo_v_pnr]" +
                        "([idanexo_v_pnr],[idnomina],[no_comprobante],[ur],[periodo],[tipo_nomina],[primer_apellido],[segundo_apellido],[nombres]," +
                        "[clave_plaza],[curp],[rfc],[fecha_pago],[fecha_inicio],[fecha_termino],[percepciones],[deducciones],[neto],[nss],[cct]," +
                        "[forma_pago],[cve_banco],[clabe],[motivo],[nivel_cm],[qna_proc],[cons_qna_proc],[num_cheque],[cheque_dv],[grupo])" +
                        "VALUES( newid(),"+rowHcc["idnomina"].ToString()+","+rowHcc["no_comprobante"].ToString() +","+rowHcc["ur"].ToString() +", '"+periodo+"', 'O','"+dtE.Rows[0]["primer_apellido"].ToString() +"','"+ dtE.Rows[0]["segundo_apellido"].ToString() + "', " +
                        "'"+ dtE.Rows[0]["nombres"].ToString() + "','"+rowHcc["plaza"].ToString()+"','"+dtEC.Rows[0]["curp"].ToString()+"','"+rowHcc["rfc"].ToString()+"','"+ QuincenaToFecha(Int32.Parse(rowHcc["fecha_pago"].ToString())) + "'," +
                        "'"+ rowHcc["fecha_inicio"].ToString() + "','"+ rowHcc["fecha_termino"].ToString() + "',"+ rowHcc["percepciones"].ToString() + "," + rowHcc["deducciones"].ToString() + "," + rowHcc["neto"].ToString() + 
                        ",'"+dtEnss.Rows[0]["nss"].ToString()+ "','" + rowHcc["cct"].ToString() + "','" + rowHcc["forma_pago"].ToString() + "','','', '" + rowHcc["motivo"].ToString() + "','" + rowHcc["nivel_cm"].ToString() +
                        "'," + rowHcc["qna_proc"].ToString() + "," + rowHcc["cons_qna_proc"].ToString() + "," + rowHcc["num_cheque"].ToString() + ",'" + rowHcc["cheque_dv"].ToString() + "','" + rowHcc["grupo"].ToString() + "')";
                    */

                    Console.WriteLine("num_perc y num_dec" + rowHcc["num_perc"] + "   " + rowHcc["num_desc"]);
                    int iterar = int.Parse(rowHcc["num_perc"].ToString()) + int.Parse(rowHcc["num_desc"].ToString());
                    for (int i = 1; i<= iterar; i++ )
                    {
                        string queryInsA6 =  "INSERT INTO [dbo].[anexo_vi_pnr] ([idanexo_vi_pnr],[idnomina],[no_comprobante],[ur],[periodo],[tipo_nomina], [clave_plaza],[curp],[tipo_concepto],[cod_concepto],[desc_concepto]," +
                            "[importe],[base_calculo_isr],[observaciones],[conciliaciones],[ministracion],[consecutivo],[qna_proc],[cons_qna_proc],[grupo]) " +
                            "VALUES(newid(),"+rowHcc["idnomina"].ToString()+","+ rowHcc["no_comprobante"].ToString() + "," + rowHcc["ur"].ToString() + "," + periodo + "," + rowHcc["t_nomina"].ToString() + "," + rowHcc["plaza"].ToString() + ",'" + dtEC.Rows[0]["curp"].ToString() + "'," +
                            "<tipo_concepto, char (1),>,<cod_concepto, char (4),>,<desc_concepto, varchar(200),>,<importe, decimal (18,2),>,<base_calculo_isr, int,>,<observaciones, varchar(200),>,<conciliaciones, varchar(200),>,<ministracion, int,>," +
                            "<consecutivo, int,>,<qna_proc, int,>,<cons_qna_proc, smallint,>,<grupo, varchar(6),>)";
                        string cifra = "" + i  ;
                        string cifra2 = "" + cifra.PadLeft(2,'0');


                        Console.WriteLine("vuelta : "+cifra2);
                        string queryCpto = " SELECT concepto, descripcion FROM ptda_concepto WHERE concepto = '"+rowHcc["concepto"+cifra2]+"'";
                        Console.WriteLine(queryCpto);
                    }
                    
                }

            }

        }




        public static (DataTable dtEC, DataRow rowHcc) FindCurp(DataTable dtEC, DataRow rowHcc)
        {
            //Console.WriteLine("Epa si fuiomos invocados");
            //Console.WriteLine("rfc "+rowHcc["rfc"]);
            string queryRfcs_Ec = "select ec.rfc, ec.cve_unica as curp from empleado_curp as ec where '" + rowHcc["rfc"] + "' = ec.rfc";
            dtEC = EjecutarSentencia(queryRfcs_Ec);
            //Console.WriteLine(dtEC.Rows.Count);
            if (dtEC.Rows.Count == 0)
            {
                //Console.WriteLine("Epaaaa no encotramos nada en empleado_curp");
                string queryRfcs_Eec = "select pec.rfc, pec.cve_unica as curp from pagos_especiales_curp as pec where '" + rowHcc["rfc"] + "' = pec.rfc";
                dtEC = EjecutarSentencia(queryRfcs_Eec);
                if (dtEC.Rows.Count == 0)
                {
                    Console.WriteLine("Epaaaaaaaaaaaaaaa tampoco encontramos en pagos especiales");
                    DataTable dtTemp = EjecutarSentencia("select rfc_nvo from cambio_rfc where rfc_anterior = '" + rowHcc["rfc"] + "'");
                    if (dtTemp.Rows.Count >= 1)
                    {
                        Console.WriteLine("rfc actualizado : " + dtTemp.Rows[0][0].ToString());
                        rowHcc["rfc"] = dtTemp.Rows[0][0].ToString();

                        var tpl = FindCurp(dtEC, rowHcc);
                        dtEC = tpl.dtEC;
                        rowHcc = tpl.rowHcc;



                    }

                }

            }

            return (dtEC, rowHcc);
        }


        private static string AcomodaPeriodo(string str)
        {
            string years = str.Substring(0, 4);
            string qna = str.Substring(4, 2);
            Console.WriteLine("periodo " + str);
            Console.WriteLine("años " + years);
            Console.WriteLine("quincena " + qna);

            string periodo = qna + years;
            Console.WriteLine("periodoOK " + periodo);
            return periodo;
        }

        public static Boolean CurpValida(String CURP)
        {
            //Console.WriteLine("Creo que vmaos bien");
            if (CURP.Length != 18)
                return false;
            ConsultaCURP consultaCURP = new ConsultaCURP();
            try
            {
                ResponseRenapo responseRenapo = consultaCURP.ConsultaPorCurp(CURP);
                if (responseRenapo.ConsultaExitosa == false)
                {
                    Console.WriteLine("-----------------------Curp Invalida : " + CURP);
                    
                    return false;
                }
                else
                {
                    Console.WriteLine("Curp validad");
                    return true;
                }
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetType().Name + ":" + ex.Message);
                return false;
            }


        }



        private static DateTime QuincenaToFecha(Int32 anioquincena)
        {
            int qna = anioquincena % 100;
            int anio = System.Convert.ToInt32(anioquincena) / 100;
            int mes = System.Convert.ToInt32(qna / 2) + qna % 2;
            int dia = qna % 2 == 0 ? DateTime.DaysInMonth(anio, mes) : 15;

            // Dim dia As Integer = IIf((qna Mod 2) = 0, Date.DaysInMonth(anio, mes), 15)
            string result = String.Format("{0}/{1}/{2}", dia, mes, anio);
            return Convert.ToDateTime(result);
        }


        private static DataTable EjecutarSentencia(string sentencia)
        {
            //Console.WriteLine("sentencia entrando : "+sentencia);
            DataTable tabla = new DataTable();
            StringBuilder errorMessages = new StringBuilder();
            try
            {
                Conexion conn = new Conexion();
                conn.Coneccion();
                conn.GetConnection().Open();
                //conn.getCommand().CommandTimeout = 6000;
                conn.Ejecutar(sentencia);
                SqlDataReader dr = conn.GetCommand().ExecuteReader();
                if (dr.HasRows)
                {
                    try
                    {
                        tabla.Load(dr);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.GetType().Name + ":" + ex.Message);
                    }
                    dr.Close();
                    conn.GetConnection().Close();
                    conn.GetCommand().Dispose();
                    return tabla;
                }
                return tabla;
            }
            catch (SqlException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message ES: " + ex.Errors[i].Message + "\n" +
                        "Error Number ES: " + ex.Errors[i].Number + "\n" +
                        "LineNumber ES: " + ex.Errors[i].LineNumber + "\n" +
                        "Source ES: " + ex.Errors[i].Source + "\n" +
                        "Procedure ES: " + ex.Errors[i].Procedure + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
                return null;
            }
        }




    }

    class Conexion
    {
        private SqlConnection connection = new SqlConnection();
        private SqlCommand command = new SqlCommand();

        public SqlConnection Coneccion()
        {
            StringBuilder errorMessages = new StringBuilder();
            try
            {
                string stringConexion = "data source=winsql;initial catalog=consultalectura;user id=udiaz;password=servicio2022!";
                //string stringConexion = "data source=localhost\\SQLEXPRESS;initial catalog=consultalectura;user id=sa;password=servicio2022!";
                connection = new SqlConnection(stringConexion);
            }
            catch (SqlException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "Error Number: " + ex.Errors[i].Number + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n" +
                        "Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
            }
            
            return connection;
        }
        public void Ejecutar(string sentencia)
        {
            //Console.WriteLine("Todo bien ?");
            command = new SqlCommand(sentencia, connection);
        }

        public SqlConnection GetConnection() { return connection; }

        public SqlCommand GetCommand() { return command; }

        public void ConnOpen()
        {
            StringBuilder errorMessages = new StringBuilder();
            try
            {
                connection.Open();
            }
            catch (SqlException ex)
            {
                for (int i = 0; i < ex.Errors.Count; i++)
                {
                    errorMessages.Append("Index #" + i + "\n" +
                        "Message: " + ex.Errors[i].Message + "\n" +
                        "Error Number: " + ex.Errors[i].Number + "\n" +
                        "LineNumber: " + ex.Errors[i].LineNumber + "\n" +
                        "Source: " + ex.Errors[i].Source + "\n" +
                        "Procedure: " + ex.Errors[i].Procedure + "\n");
                }
                Console.WriteLine(errorMessages.ToString());
            }
            
        }

        public void ConnClose()
        {
            connection.Close();
        }
    }
}