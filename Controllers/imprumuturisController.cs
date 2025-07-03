using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using carp_tm_2025.Data;
using carp_tm_2025.Models;
using Excel.FinancialFunctions;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Geom;
using System.Drawing.Printing;
using X.PagedList.Extensions;
using Microsoft.IdentityModel.Tokens;
using static iText.IO.Util.IntHashtable;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Events;
using iText.Layout.Layout; 
using iText.Layout.Renderer;
using System.IO;
using iText.Layout.Borders;
using iText.Commons.Utils;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using iText.IO.Font;
using Humanizer;
using static Org.BouncyCastle.Utilities.Test.FixedSecureRandom;
using System.Diagnostics.Contracts;



namespace carp_tm_2025.Controllers
{
    [Authorize(Roles = "CARP_TM\\CARP_TM_READ")]
    public class imprumuturisController : Controller
    {
        private readonly carp_tm_2025Context _context;
        private readonly IWebHostEnvironment _env;


        public static string titlu1_raport_impr;
        public static string titlu2_raport_impr;

        public imprumuturisController(carp_tm_2025Context context, IWebHostEnvironment environment)
        {
            _context = context;
            _env = environment;

        }

        // GET: imprumuturis
        public async Task<IActionResult> Index(int searchterm = 99999)
        {
            var imprumutnrcarnet = await _context.imprumuturis
                          .Where(p => p.nrcarnet == searchterm).OrderByDescending(p => p.data_imprumut).ToListAsync();

            ViewData["partida"] = searchterm;            

            return View(imprumutnrcarnet);
        }             
    
        
        // GET: imprumuturis/Create
        public IActionResult Create(int idp)
        {
            ViewData["datanew"] = DateTime.Now;

            var getnume = _context.Pensionars
                .Where(p => p.nrcarnet == idp).SingleOrDefault();
           
            ViewData["nume"] = getnume.nume;
            ViewData["partidac"] = idp;         

            return View();
        }
         
        // POST: imprumuturis/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("idimprum,nrcarnet,valoare_imprumut,data_imprumut,nume,nr_contract,mod_acordare")] imprumuturi imprumuturi)
        {


            int ang_nou = 0;


            var cek_desf_new =await _context.desfasurator_rate
                .Where(cdn => cdn.nrcarnet == imprumuturi.nrcarnet).ToListAsync();
            if (cek_desf_new.Count() >= 1)
            {
                ViewBag.msgerr = "Mai exista un desfasurator pentru acest nr de carnet";
                return View("Error");
            }

            ang_nou = 0;
            var pens_impr_nou = _context.Pensionars
                .Where(pin => pin.nrcarnet == imprumuturi.nrcarnet).SingleOrDefault();
            if (pens_impr_nou.nume.Substring(0, 6) == "C.A.R.") ang_nou = 1;





            if (ModelState.IsValid)
            {
                _context.Add(imprumuturi);
                await _context.SaveChangesAsync();




                update_pensionar_impr(imprumuturi.nrcarnet, imprumuturi.valoare_imprumut, imprumuturi.data_imprumut);


                update_registru_plati(imprumuturi.idimprum, imprumuturi.data_imprumut, imprumuturi.nrcarnet, imprumuturi.nume, imprumuturi.valoare_imprumut, imprumuturi.nr_contract, imprumuturi.mod_acordare);


                introducere_desfasurator_imprumut_nou(imprumuturi.nrcarnet, imprumuturi.idimprum, imprumuturi.valoare_imprumut, ang_nou, imprumuturi.nr_contract);

                corectie_001(imprumuturi.nrcarnet, imprumuturi.valoare_imprumut);



                return  RedirectToAction("Index", new { searchterm = imprumuturi.nrcarnet });
            }
            return View(imprumuturi);
        }

        [Authorize(Roles = "CARP_TM\\CARP_TM_WRITE")]
        // GET: imprumuturis/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var imprumuturi = await _context.imprumuturis.FindAsync(id);
            if (imprumuturi == null)
            {
                return NotFound();
            }
            return View(imprumuturi);
        }

        // POST: imprumuturis/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "CARP_TM\\CARP_TM_WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("idimprum,nrcarnet,valoare_imprumut,data_imprumut,nume,nr_contract,mod_acordare")] imprumuturi imprumuturi)
        {
            if (id != imprumuturi.idimprum)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(imprumuturi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!imprumuturiExists(imprumuturi.idimprum))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(imprumuturi);
        }
              

        public async Task<IActionResult> afisare_desfasurator_rate(string id_impr_ad, string nr_carnet_ad)
        {            

            int max_nr_desfasurator = 0;
            int testad = 0, nr_carnet_ad_OK, id_impr_ad_OK = 0;
            DateTime data_scadenta_ad;
            DateTime data_imp_ad;
            int nrrate_ad = 0;
            int nr_rate_d_1 = 0;
            string nume_dr;
            decimal total_dobanda_nou = 0;

            id_impr_ad_OK = Int32.Parse(id_impr_ad);
            nr_carnet_ad_OK = Int32.Parse(nr_carnet_ad);

            var pens_show_desf =await _context.Pensionars
                .Where(psd => psd.nrcarnet == nr_carnet_ad_OK).SingleOrDefaultAsync();
            data_imp_ad = (DateTime)pens_show_desf.dataimp;
            nume_dr = pens_show_desf.nume;

            if (data_imp_ad.Year >= 2023) if (pens_show_desf.acimp <= 3000) nrrate_ad = 24;

            if (data_imp_ad.Year >= 2023) if ((pens_show_desf.acimp > 3000) && (pens_show_desf.acimp <= 10000)) nrrate_ad = 36;

            if (data_imp_ad.Year >= 2023) if (pens_show_desf.acimp > 10000) nrrate_ad = 60;


            if (data_imp_ad.Year < 2023)
            {
                if (pens_show_desf.acimp <= 3000)
                {
                    data_scadenta_ad = data_imp_ad.AddMonths(24);
                    nrrate_ad = (data_scadenta_ad.Year * 12 + data_scadenta_ad.Month) - (2022 * 12 + 12);
                }

                if ((pens_show_desf.acimp > 3000) && (pens_show_desf.acimp <= 10000))
                {
                    data_scadenta_ad = data_imp_ad.AddMonths(36);
                    nrrate_ad = (data_scadenta_ad.Year * 12 + data_scadenta_ad.Month) - (2022 * 12 + 12);
                }

                if (pens_show_desf.acimp > 10000)
                {
                    data_scadenta_ad = data_imp_ad.AddMonths(60);
                    nrrate_ad = (data_scadenta_ad.Year * 12 + data_scadenta_ad.Month) - (2022 * 12 + 12);
                }
            }

            var get_desf_afisare =await _context.desfasurator_rate
                .Where(gda => gda.id_imprumut == id_impr_ad_OK).OrderBy(gda => gda.nr_rata).ToListAsync();
            if (get_desf_afisare.Count() == 0) testad = 1;

                       

            if (testad == 1)
                return RedirectToAction("afisare_desfasurator_rate_arhiva", new { id_impr_adArh = id_impr_ad_OK, nr_carnet_adArh = nr_carnet_ad_OK });


            if (get_desf_afisare.Count() > 0)
                max_nr_desfasurator = get_desf_afisare.Max(gda => gda.nr_desfasurator);

            ViewData["nr_desfasurator_max"] = max_nr_desfasurator;

            if (max_nr_desfasurator > 0)
                return RedirectToAction("afisare_grafic_rambursare_nou", new { nr_desfasurator = max_nr_desfasurator, idimpr_show_change_d = id_impr_ad_OK, nrcarnet_show_change_d = nr_carnet_ad_OK });


            ViewData["nrcarnet_ad"] = nr_carnet_ad;
            ViewData["nrrate_ad"] = nrrate_ad;
            ViewData["idimprum"] = id_impr_ad_OK;
            ViewData["nume_dr"] = nume_dr;


            total_dobanda_nou = get_desf_afisare.Sum(gda => gda.dobanda);
            ViewData["total_dobanda"] = total_dobanda_nou.ToString("N2");
            ViewData["total_rata"] = get_desf_afisare.Sum(gda => gda.rata_de_plata).ToString("N2");
            ViewData["total_total"] = get_desf_afisare.Sum(gda => gda.total).ToString("N2");


            if (data_imp_ad.Year < 2023)
                ViewData["sold_impr_ad"] = pens_show_desf.soldimp2016.ToString("N2");

            if (data_imp_ad.Year >= 2023)
                ViewData["sold_impr_ad"] = pens_show_desf.acimp.ToString("N");         
          

            return View(get_desf_afisare);
            //paTutorial
        }
        public async Task<ActionResult> afisare_desfasurator_rate_arhiva(string id_impr_adArh, string nr_carnet_adArh)
        {

            //mart 2023
            int max_nr_desfasurator = 0;
            int testad = 0, nr_carnet_adArh_OK, id_impr_adArh_OK = 0;
            DateTime data_scadenta_ad;
            DateTime data_imp_adArh;
            int nrrate_adArh = 0;
            int nr_rate_d_1 = 0;
            string nume_dr;
            decimal total_dobandaDaArh = 0;
            id_impr_adArh_OK = Int32.Parse(id_impr_adArh);
            nr_carnet_adArh_OK = Int32.Parse(nr_carnet_adArh);
            var impr_show_desf_arh = await _context.imprumuturis
                .Where(isda => isda.idimprum == id_impr_adArh_OK).SingleOrDefaultAsync();
            data_imp_adArh = (DateTime)impr_show_desf_arh.data_imprumut;
            nume_dr = impr_show_desf_arh.nume;


            var get_desf_afisare_arh =await _context.desfasurator_rate_arhiva
                 .Where(gda => gda.id_imprumut == id_impr_adArh_OK).OrderBy(gdaa => gdaa.nr_rata).ToListAsync();

            nrrate_adArh = get_desf_afisare_arh.Count();

            ViewData["nrcarnet_adArh"] = nr_carnet_adArh_OK;
            ViewData["nrrate_adArh"] = nrrate_adArh;
            ViewData["idimprumArh"] = id_impr_adArh_OK;
            ViewData["nume_drArh"] = nume_dr;
            //ok pa cek code

            total_dobandaDaArh = get_desf_afisare_arh.Sum(gda => gda.dobanda);
            ViewData["total_dobandaAdArh"] = total_dobandaDaArh.ToString("N2");
            ViewData["total_rataAdArh"] = get_desf_afisare_arh.Sum(gda => gda.rata_de_plata).ToString("N2");
            ViewData["total_totalAdArh"] = get_desf_afisare_arh.Sum(gda => gda.total).ToString("N2");

            ViewData["sold_impr_adArh"] = impr_show_desf_arh.valoare_imprumut.ToString("N2");


            return View(get_desf_afisare_arh);

        }

        public async Task<ActionResult> afisare_grafic_rambursare_nou(int nr_desfasurator, int idimpr_show_change_d, int nrcarnet_show_change_d)
        {
            //8 martie

            ViewData["idimpr_d_before"] = idimpr_show_change_d;


            ViewData["nr_desfasurator"] = nr_desfasurator;
            ViewData["nr_desfasurator_max"] = nr_desfasurator;

            var desf_change_show =await _context.desfasurator_rate.Where(dscd => (dscd.nr_desfasurator == nr_desfasurator) && (dscd.nrcarnet == nrcarnet_show_change_d)).OrderBy(dscd => dscd.data_rata).ToListAsync();

            ViewData["nr_carnet_change_d"] = nrcarnet_show_change_d;


            ViewData["soldimp_change_d"] = desf_change_show.First().sold_imp + desf_change_show.First().rata_de_plata;
            ViewData["nrrate_change_d"] = desf_change_show.Count();


            ViewData["total_dobanda_chd"] = desf_change_show.Sum(dscd => dscd.dobanda);
            ViewData["total_total_chd"] = desf_change_show.Sum(dscd => dscd.total);


            return View(await _context.desfasurator_rate.Where(dscd => (dscd.id_imprumut == idimpr_show_change_d) && (dscd.nr_desfasurator == nr_desfasurator) && (dscd.nrcarnet == nrcarnet_show_change_d)).OrderBy(dscd => dscd.data_rata).ToListAsync());
        }

        public async Task<ActionResult> desfasurator_rate_imprumut_anterior(int nr_desfasurator, int nr_desfasurator_max, int idimpr_d_before, int nr_carnet_d_before)
        {
            //8 martie
            ViewData["nr_desfasurator"] = nr_desfasurator;
            ViewData["idimpr_show_change_d"] = idimpr_d_before;
            ViewData["nr_desfasurator_max"] = nr_desfasurator_max;

            int nrrate_d = 0, nr_rate_total = 0;


            double rata_d_before = 0, dobanda_da = 0;
            double soldimp_d_before = 0;
            double dob_d_before = 0;
            int nrrate_d_before = 0;
            DateTime data_d_before;

            int nrr_da = 0;

            string pdfDRA = "";

            double total_dobanda_da = 0;
            double total_total_da = 0;
            double total_rata_da = 0;

            int nr_rate_d_1 = 0;


            var get_date_desf_modif =await _context.desfasurator_rate_modificari
                 .Where(gddm => (gddm.id_imprumut == idimpr_d_before) && (gddm.nrcarnet == nr_carnet_d_before) && (gddm.nr_desfasurator == nr_desfasurator - 1)).SingleOrDefaultAsync();

            soldimp_d_before = (double)get_date_desf_modif.sold_imprumut_modificat;
            data_d_before = get_date_desf_modif.data_modificare;
            dob_d_before = (double)get_date_desf_modif.dobanda_imprumut;
            nrrate_d_before = get_date_desf_modif.nr_rate;


            int nrdaymonth = DateTime.DaysInMonth(data_d_before.Year, data_d_before.Month);
            int nrdaydiff = nrdaymonth - data_d_before.Day;
            data_d_before = data_d_before.AddDays(nrdaydiff);


            var desfasurator_rate_anterior = new List<desfasurator_rate_anterior>();

            //pa


            ViewData["nrrateDA"] = nrrate_d_before;
            ViewData["sold_imprDA"] = soldimp_d_before.ToString("N2");
            ViewData["nrcarnetDA"] = nr_carnet_d_before;

            rata_d_before = Financial.Pmt(dob_d_before / 12, nrrate_d_before, -soldimp_d_before, 0, PaymentDue.EndOfPeriod);
            rata_d_before = round_value(rata_d_before);

            nrr_da = 0;

            pdfDRA = " ";

            pdfDRA += "<table  cellspacing='0' border='1' cellpadding='3' >" +
                       "<tr>" +
                       "<td style='height:9px; text-align:center'><b>nr rata</b></td>" +
                       "<td style='height:9px; text-align:center'><b> data rata</b></td>" +
                       "<td style='height:9px; text-align:center'><b>sold ramas</b></td>" +
                       "<td style='height:9px; text-align:center'><b>rata</b></td>" +
                       "<td style='height:9px; text-align:center'><b>dobanda</b></td>" +
                       "<td style='height:9px; text-align:center'><b>total</b></td>" +

                       "</tr>";

            nr_rate_d_1 = nrrate_d;
            //pa cek code

            while (nrrate_d_before > 0)
            {
                dobanda_da = (soldimp_d_before * dob_d_before * 30) / 360;
                dobanda_da = round_value(dobanda_da);

                total_dobanda_da = total_dobanda_da + dobanda_da;

                nrr_da = nrr_da + 1;
                nrrate_d_before = nrrate_d_before - 1;

                if ((rata_d_before - dobanda_da) > soldimp_d_before)
                {
                    rata_d_before = dobanda_da + soldimp_d_before;
                    soldimp_d_before = 0;
                };

                if ((rata_d_before - dobanda_da) <= soldimp_d_before)
                    soldimp_d_before = soldimp_d_before - (rata_d_before - dobanda_da);

                if ((nrrate_d_before == 0) && (soldimp_d_before > 0))
                {
                    rata_d_before = rata_d_before + soldimp_d_before;
                    soldimp_d_before = 0;
                }

               

                desfasurator_rate_anterior.Add(new desfasurator_rate_anterior
                {
                    nr_rata = nrr_da,
                    data_rata = data_d_before.AddMonths(nrr_da),
                    sold_imp = (decimal)soldimp_d_before,
                    rata_de_plata = (decimal)(rata_d_before - dobanda_da),
                    dobanda = (decimal)dobanda_da,
                    total = (decimal)rata_d_before
                });

               _context.SaveChanges();

                total_rata_da = total_rata_da + (rata_d_before - dobanda_da);
                total_total_da = total_total_da + rata_d_before;

                pdfDRA += "<tr>" + "<td style='height:5px; text-align:center;font-size:8'>" + nrr_da.ToString() + " " + "</td>" +
                                   "< td style = 'height:5px; text-align:center;font-size:8' > " + data_d_before.AddMonths(nrr_da).ToString("dd.MM.yyyy") + " " + " </ td > " +
                                   "<td style='height:5px; text-align:center;font-size:8'>" + soldimp_d_before.ToString("N") + "</td>" +
                                   "<td style='height:5px; text-align:center;font-size:8'>" + (rata_d_before - dobanda_da).ToString("N") + "</td>" +
                                   "<td style='height:5px; text-align:center;font-size:8'>" + dobanda_da.ToString("N") + "</td>" +
                                   "<td style='height:5px; text-align:center;font-size:8'>" + rata_d_before.ToString("N") + " " + "</td>" +
                                   "</tr>";
            }


            pdfDRA = pdfDRA + "</table>";


            //dtRC = ziuaOKl;
            //titluraportRC = "Desfasurator rate pentru valoar imprumut, varianta 1, varianta 2Incasari ("{0:d/M/yyyy}", dtRC);
            //pdfWriter.PageEvent = new ITextEvents();

            ViewData["nrcarnet_ada"] = nr_carnet_d_before;

            ViewData["total_dobandaDA"] = total_dobanda_da.ToString("N2");
            ViewData["total_rataDA"] = total_rata_da.ToString("N2");
            ViewData["total_totalDA"] = total_total_da.ToString("N2");


            return View(desfasurator_rate_anterior);

        }

        public double round_value(double dbnda)   
        {
            //impr 2025
            //Math.Round(3.32, 1, MidpointRounding.AwayFromZero) 
            double rest = dbnda - Math.Truncate(dbnda);

            int rest1 = 99;
            int rest2 = 88;

            if (rest.ToString().Length > 4)
            {
                rest1 = Convert.ToInt32(rest.ToString().Substring(2, 2));
                rest2 = Convert.ToInt32(rest.ToString().Substring(4, 1));

                if (rest2 >= 5) rest1 = rest1 + 1;

                if (rest1 != 100) dbnda = Convert.ToDouble(Math.Truncate(dbnda).ToString() + "." + rest1);
                if (rest1 == 100) dbnda = Convert.ToDouble((Math.Truncate(dbnda) + 1).ToString() + ".00");


                if (Convert.ToInt32(rest.ToString().Substring(2, 1)) == 0) dbnda = Convert.ToDouble(Math.Truncate(dbnda).ToString() + ".0" + rest1);
            }

            return dbnda;

        }
              

        public int introducere_desfasurator_imprumut_nou(int nr_carnet_idn, int id_imprumut_idn, decimal val_impr_idn, int este_ang, int nrcontract)
        {
            //sept2024-tbl

         

            double total_RP = 0;

            DateTime data_scadenta_d;
            int nrrate_d = 0, nr_rate_total = 0;
            double rata = 0, rata_OK = 0, rataN = 0, rata_OKN = 0;
            DateTime data_imp_d;
            double sold_imp_d, dobanda_d = 0, sold_imp_OK = 0;
            int id_imprum = 0;
            int nrr = 0;
            decimal getdob = 0;
            int iddob = 0;



            double total_dobanda = 0;


            DateTime now = DateTime.Now;

            int nr_lastDI = 0;
            nr_lastDI = 0;
            if ((DateTime.Now.Month == 11) || (DateTime.Now.Month == 2) || (DateTime.Now.Month == 4) || (DateTime.Now.Month == 6) || (DateTime.Now.Month == 9))
            {
                now = now.AddMonths(1);
                nr_lastDI = 1;
            }

                       

            int nrdaymonth = DateTime.DaysInMonth(now.Year, now.Month);
            int nrdaydiff = nrdaymonth - now.Day;

            now = now.AddDays(nrdaydiff);



            data_imp_d = DateTime.Now;

            sold_imp_d = decimal.ToDouble(val_impr_idn);

            if (val_impr_idn <= 1000) nrrate_d = 12;

            if ((val_impr_idn > 1000) && (val_impr_idn <= 3000)) nrrate_d = 24;

            if ((val_impr_idn > 3000) && (val_impr_idn <= 10000)) nrrate_d = 36;

            if (val_impr_idn > 10000) nrrate_d = 60;


            if (sold_imp_d <= 1000)
            {
                rata = Financial.Pmt(0.050 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.05M;
            }


            if ((sold_imp_d > 1000) && (sold_imp_d <= 3000))
            {
                rata = Financial.Pmt(0.06 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.07M;
            }


            if ((sold_imp_d > 3000) && (sold_imp_d <= 10000))
            {
                rata = Financial.Pmt(0.075 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.075M;
            }

            if ((sold_imp_d > 10000) && (sold_imp_d <= 25000))
            {
                rata = Financial.Pmt(0.095 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.095M;
            }

            if ((sold_imp_d > 25000) && (sold_imp_d <= 35000))
            {
                rata = Financial.Pmt(0.105 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.105M;
            }



            if ((sold_imp_d > 35000) && (sold_imp_d <= 49000))
            //if (sold_imp_d == 49000)
            {
                rata = Financial.Pmt(0.115 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.115M;
            }


            if (sold_imp_d == 70000)
            {
                rata = Financial.Pmt(0.12 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.12M;
            }

            if (este_ang == 1)
            {
                rata = Financial.Pmt(0.05 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.05M;

            }



            rata = round_value(rata);

            nrr = 0;

            total_dobanda = 0;
            total_RP = 0;
            while (nrrate_d > 0)
            {
                dobanda_d = (sold_imp_d * decimal.ToDouble(getdob) * 30) / 360;
                dobanda_d = round_value(dobanda_d);

                total_dobanda = total_dobanda + dobanda_d;


                nrr = nrr + 1;
                nrrate_d = nrrate_d - 1;

                if ((rata - dobanda_d) > sold_imp_d)
                {
                    rata = dobanda_d + sold_imp_d;
                    sold_imp_d = 0;
                };

                if ((rata - dobanda_d) <= sold_imp_d)
                    sold_imp_d = sold_imp_d - (rata - dobanda_d);

                if ((nrrate_d == 0) && (sold_imp_d > 0))
                {
                    rata = rata + sold_imp_d;
                    sold_imp_d = 0;
                }


                //pa cek code, 2 pb zi rata 31 si prag 1000

                _context.desfasurator_rate.Add(new desfasurator_rate
                {
                    id_imprumut = id_imprumut_idn,
                    nrcarnet = nr_carnet_idn,
                    nr_rata = nrr,
                    data_rata = now.AddMonths(nrr - nr_lastDI),                    
                    sold_imp = (decimal)sold_imp_d,
                    rata_de_plata = (decimal)(rata - dobanda_d),
                    rata_platita = 0,
                    rata_credit = 0,
                    dobanda = (decimal)dobanda_d,
                    total = (decimal)rata
                });

                _context.SaveChanges();


                total_RP = total_RP + (rata - dobanda_d);

            }


            //de aici dec 2023
            _context.registru_jurnal.Add(new registru_jurnal
            {
                data = DateTime.Now,
                id_document = id_imprumut_idn,
                nr_document = "_nr_ctrct._impr." + nrcontract.ToString(),
                id_CS_debitor = 109,
                id_CA_debitor = Int32.Parse("6791" + nr_carnet_idn.ToString()),
                id_CS_creditor = 53,
                id_CA_creditor = 103,
                explicatie_nr_cont_analitic = "-",
                SI_CS_debit = 0,
                SI_CA_debit = 0,
                SI_CS_credit = 0,
                SI_CA_credit = 0,
                debit = (decimal)total_dobanda,
                credit = (decimal)total_dobanda,
                SF_CS_debit = 0,
                SF_CA_debit = (decimal)total_dobanda,
                SF_CS_credit = 0,
                SF_CA_credit = 0,
                tip_document = 11,
                sortare = "A"

            });



            var pensUpSolDob = _context.Pensionars.Where(pusd => pusd.nrcarnet == nr_carnet_idn).SingleOrDefault();
            pensUpSolDob.solddobanda = (decimal)total_dobanda;
            _context.SaveChanges();


            iddob = Int32.Parse("6791" + pensUpSolDob.nrcarnet.ToString());
            var checkIDob = _context.conturi_analitice
                .Where(cic => cic.id_cont_analitic == iddob);
            if (checkIDob.Count() == 0)
            {

                _context.conturi_analitice.Add(new conturi_analitice
                {

                    id_cont_analitic = Int32.Parse("6791" + pensUpSolDob.nrcarnet.ToString()),
                    id_cont_sintetic = 109,
                    nr_cont_analitic = "26791." + pensUpSolDob.nrcarnet.ToString(),
                    explicatie_nr_cont_analitic = pensUpSolDob.nume

                });
                _context.SaveChanges();

            }


            return 1;

        }

        public int update_registru_plati(int id_impr, DateTime dataimpr, int nrcarnet, string nume, decimal valoare_imprumut, int nr_contract, int tip_document_rj_imp)
        {
            //sept2024-tbl
            int id_CS_imp = 0;
            int id_CA_imp = 0;


            id_CS_imp = 51;
            id_CA_imp = 101;

            if (tip_document_rj_imp == 1)
            {
                id_CS_imp = 52;
                id_CA_imp = 102;
            }


            var pensrcp = _context.Pensionars
               .Where(prcp => prcp.nrcarnet == nrcarnet).SingleOrDefault();


           

            _context.registru_jurnal.Add(new registru_jurnal
            {
                data = dataimpr,
                id_document = id_impr,
                nr_document = "_nr_ctrct._impr." + nr_contract,
                id_CS_debitor = 103,
                id_CA_debitor = Int32.Parse("2678" + nrcarnet.ToString()),
                id_CS_creditor = id_CS_imp,
                id_CA_creditor = id_CA_imp,
                explicatie_nr_cont_analitic = "-",
                SI_CS_debit = 0,
                SI_CA_debit = 0,
                SI_CS_credit = 0,
                SI_CA_credit = 0,
                debit = valoare_imprumut,
                credit = valoare_imprumut,
                SF_CS_debit = 0,
                SF_CA_debit = valoare_imprumut,
                SF_CS_credit = 0,
                SF_CA_credit = 0,
                tip_document = tip_document_rj_imp,
                sortare = "A"
            });

            _context.SaveChanges();



            //de aici dob





            return 0;

        }

        public int update_pensionar_impr(int nrcarnet, decimal debit, DateTime dataimp)
        {

            //sept2024-tbl

            var pens = (from p in _context.Pensionars
                        where ((p.nrcarnet == nrcarnet))
                        select p).FirstOrDefault();
            pens.soldimp = debit;
            pens.acimp = debit;
            pens.restimp = 0;
            pens.lunaimp = dataimp;
            pens.dataimp = dataimp;

            _context.SaveChanges();
            return 0;
        }

        public int corectie_001(int nrcarnet_repar001, decimal valoare_imprumut_repar001)
        {

            decimal total_RP = 0;
            double total_RP_Dbl = 0;


            total_RP = _context.desfasurator_rate.Where(drgetRP => drgetRP.nrcarnet == nrcarnet_repar001).Sum(drgetRP => drgetRP.rata_de_plata);

            total_RP_Dbl = (double)total_RP;
            total_RP_Dbl = round_value(total_RP_Dbl);

            total_RP = (decimal)total_RP_Dbl;


            if (valoare_imprumut_repar001 - total_RP == 0.01M)
            {
                var desfRepar001 = _context.desfasurator_rate.Where(dr001 => dr001.nrcarnet == nrcarnet_repar001).OrderByDescending(dr001 => dr001.data_rata).First();

                desfRepar001.rata_de_plata = desfRepar001.rata_de_plata + 0.01M;
                desfRepar001.total = desfRepar001.total + 0.01M;
                _context.SaveChanges();
            }

            return 1;
        }

        public async Task<IActionResult> listare_desfasurator(string id_imprumut_ld, string nr_carnet_ld, string nr_desfasurator_l)
        {

            //merge am testat creaza pdf
            // PdfWriter writer = new PdfWriter("C:\\inetpub\\wwwroot\\carp_tm_2022\\carp_tm_2022\\pdf\\demo.pdf");
            //PdfDocument pdfDoc = new PdfDocument(writer);
            //Document document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A4, false);
            //end merge


            // 9 sept2024 tbl
            int max_nr_desfasurator_l = 0, nr_rata_l = 0;

            string pdfdr = "";
            int id_imprumut_ld_OK;
            int nrcarnet_ld_OK;
            int i = 0;
            int test = 0;
            decimal sldi = 0;
            decimal total_dobanda = 0, total_rata = 0, total_total = 0;
            string nume_ld;
            int nr_desfasurator_l_ok = 0;

            id_imprumut_ld_OK = Int32.Parse(id_imprumut_ld);
            nrcarnet_ld_OK = Int32.Parse(nr_carnet_ld);
            nr_desfasurator_l_ok = Int32.Parse(nr_desfasurator_l);

            var pens_ld =await _context.Pensionars
                .Where(pld => pld.nrcarnet == nrcarnet_ld_OK).SingleOrDefaultAsync();
            nume_ld = pens_ld.nume;


            var get_desf =await _context.desfasurator_rate
                .Where(gd => (gd.id_imprumut == id_imprumut_ld_OK) && (gd.nr_desfasurator == nr_desfasurator_l_ok)).OrderBy(gd => gd.nr_rata).ToListAsync();

            if (get_desf.Count() == 0) test = 1;


            if (test == 1)
                get_desf =await _context.desfasurator_rate
                    .Where(gd => gd.nrcarnet == nrcarnet_ld_OK).OrderBy(gd => gd.nr_rata).ToListAsync();
                       
            //de aici listare

            MemoryStream ms = new MemoryStream();
            PdfWriter writer = new PdfWriter(ms);
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A4, false);
            writer.SetCloseStream(false);

            document.SetMargins(100f, 20f, 50f, 20f);

            Style normal = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normal.SetFont(font).SetFontSize(14);

            //header
            Paragraph header1 = new Paragraph("C.A.R. Pensionari Turnu Magurele")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(13).AddStyle(normal); 
            
            Paragraph header2 = new Paragraph("cod fiscal 7230589")
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(13)
                .AddStyle(normal);

         
            //
            var table = new Table(6, true);
            //iText.Layout.Element.Table table = new iText.Layout.Element.Table(6, true);
            Style normalTbl = new Style();
            PdfFont fontTbl = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTbl.SetFont(fontTbl).SetFontSize(13);


            i = 0;        
            foreach (desfasurator_rate dr in get_desf)
            {
                if (nr_desfasurator_l_ok > 0) nr_rata_l = dr.nr_rata_noua;
                if (nr_desfasurator_l_ok == 0) nr_rata_l = dr.nr_rata;
                            

                Cell nr_rata = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                         .Add(new Paragraph(dr.nr_rata.ToString()).AddStyle(normalTbl));
                //nr_rata.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
                // nr_rata.SetHeight(15);
                table.AddCell(nr_rata);

               
              
                Cell data_rata = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                         .Add(new Paragraph(dr.data_rata.ToString("dd.MM.yyyy")).AddStyle(normalTbl));
                //data_rata.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
                //data_rata.SetHeight(15);
                table.AddCell(data_rata);

                
                Cell soldimpr = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.RIGHT)
                         .Add(new Paragraph(dr.sold_imp.ToString("N2")).AddStyle(normalTbl));
                //soldimpr.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
                //soldimpr.SetHeight(15);
                table.AddCell(soldimpr);

               
                Cell rata = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.RIGHT)
                         .Add(new Paragraph(dr.rata_de_plata.ToString("N2")).AddStyle(normalTbl));
                //rata.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
                //rata.SetHeight(15);
                table.AddCell(rata);

                                
                Cell dobanda = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.RIGHT)
                         .Add(new Paragraph(dr.dobanda.ToString("N2")).AddStyle(normalTbl));
                //dobanda.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
                //dobanda.SetHeight(15);
                table.AddCell(dobanda);


                Cell total = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.RIGHT)
                         .Add(new Paragraph(dr.total.ToString("N2")).AddStyle(normalTbl));
                //total.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
                //total.SetHeight(15);
                table.AddCell(total);


                i = i + 1;

                if (nr_rata_l == 1) sldi = dr.sold_imp + dr.rata_de_plata;

            }

            Cell nr_rata_titlu = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.CENTER)
                   .Add(new Paragraph("nr rata")).AddStyle(normalTbl);

            Cell data_rata_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("data rata")).AddStyle(normalTbl);

            Cell soldimpr_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("sold impr.")).AddStyle(normalTbl);

            Cell rata_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("rata")).AddStyle(normalTbl);

            Cell dobanda_titlu = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.CENTER)
                   .Add(new Paragraph("dobanda")).AddStyle(normalTbl);

            Cell total_titlu = new Cell(1, 1)
                  .SetTextAlignment(TextAlignment.CENTER)
                   .Add(new Paragraph("total")).AddStyle(normalTbl);


            table.AddHeaderCell(nr_rata_titlu);
            table.AddHeaderCell(data_rata_titlu);
            table.AddHeaderCell(soldimpr_titlu);
            table.AddHeaderCell(rata_titlu);
            table.AddHeaderCell(dobanda_titlu);
            table.AddHeaderCell(total_titlu);


            total_dobanda = get_desf.Sum(gd => gd.dobanda);
            total_rata = get_desf.Sum(gd => gd.rata_de_plata);
            total_total = get_desf.Sum(gd => gd.total);
                        
            Cell col1F = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("Total").AddStyle(normalTbl)).SetBold();            
            table.AddCell(col1F);

            Cell col2F = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .Add(new Paragraph("").AddStyle(normalTbl)).SetBold();            
            table.AddCell(col2F);

            Cell col3F = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.RIGHT)
                     .Add(new Paragraph("").AddStyle(normalTbl)).SetBold();              
            table.AddCell(col3F);
                        
            Cell rataT = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.RIGHT)
                     .Add(new Paragraph(total_rata.ToString("N2")).AddStyle(normalTbl)).SetBold();                
            table.AddCell(rataT);

                       
            Cell dobandaT = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.RIGHT)
                     .Add(new Paragraph(total_dobanda.ToString("N2")).AddStyle(normalTbl)).SetBold();                            
            table.AddCell(dobandaT);
                     
            Cell totalT = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.RIGHT)
                     .Add(new Paragraph(total_total.ToString("N2")).AddStyle(normalTbl)).SetBold();
           table.AddCell(totalT);

          
            document.Add(table);
            table.Complete();

            titlu1_raport_impr = "Desfasurator rate pentru " + pens_ld.nume.Trim() + " ,nrcarnet " + pens_ld.nrcarnet;
            titlu2_raport_impr = "Sold imprumut " + sldi.ToString("N2") + ", nr rate " + i;


            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));

            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));
            document.Add(new Paragraph(" "));

            Paragraph footer1 = new Paragraph("Reprezentant C.A.R. Pensionari Turnu Magurele" + "                                                 "   + "  IMPRUMUTAT")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetBold().SetFontSize(13);
            document.Add(footer1);


            string h = header1 + "\n" + header2;

            i = 1;
            for ( i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                // float x = pageSize.GetWidth() / 2;
                float x = 20f;
                float y = pageSize.GetTop() - 20;
                document.ShowTextAligned(header1, x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
                document.ShowTextAligned(header2, x, y-20f, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);                              
            }


            int numberOfPages = pdfDoc.GetNumberOfPages();

            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            { 
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                // float x = pageSize.GetWidth() / 2;
                float y = 20f;
                float x = pageSize.GetBottom()+20 ;
                document.ShowTextAligned(new Paragraph(DateTime.Now.ToString("dd.MM.yyyy") + "                                                                                                                             " + "pagina " + i + " din " + numberOfPages), x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
                
            }

            TableHeaderEventHandler handler = new TableHeaderEventHandler(document);
            pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);

            document.Close();
            byte[] byteInfo = ms.ToArray();
            ms.Write(byteInfo, 0, byteInfo.Length);
            ms.Position = 0;

            byte[] byte1 = ms.ToArray();
           
            

            FileStreamResult fileStreamResult = new FileStreamResult(ms, "application/pdf");

            return fileStreamResult;


            //ok am cek all code
        }

        private class TableHeaderEventHandler : IEventHandler
        {
            private Table table;
            private float tableHeight;
            private Document doc;

           

            public TableHeaderEventHandler(Document doc)
            {
                this.doc = doc;
                InitTable();

                TableRenderer renderer = (TableRenderer)table.CreateRendererSubTree();
                renderer.SetParent(new DocumentRenderer(doc));

                // Simulate the positioning of the renderer to find out how much space the header table will occupy.
                LayoutResult result = renderer.Layout(new LayoutContext(new LayoutArea(0, PageSize.A4)));
                tableHeight = result.GetOccupiedArea().GetBBox().GetHeight();
            }

            public void HandleEvent(Event currentEvent)
            {
                PdfDocumentEvent docEvent = (PdfDocumentEvent)currentEvent;
                PdfDocument pdfDoc = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                PdfCanvas canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);
                PageSize pageSize = pdfDoc.GetDefaultPageSize();
                float coordX = pageSize.GetX() + doc.GetLeftMargin();
                float coordY = pageSize.GetTop() - doc.GetTopMargin();
                float width = pageSize.GetWidth() - doc.GetRightMargin() - doc.GetLeftMargin();
                float height = GetTableHeight();
                Rectangle rect = new Rectangle(coordX, coordY, width, height);

                new Canvas(canvas, rect)
                    .Add(table)
                    .Close();
            }

            public float GetTableHeight()
            {
                return tableHeight;
            }

            private void InitTable()
            {
                Style normalh = new Style();
                PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
                normalh.SetFont(font).SetFontSize(14);


                Cell h3 = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                         .Add(new Paragraph(titlu1_raport_impr).SetFontSize(13).SetBold().AddStyle(normalh));
                h3.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
              
                Cell h4 = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                        .Add(new Paragraph(titlu2_raport_impr).SetFontSize(13).SetBold().AddStyle(normalh));
                h4.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);

                Cell h5 = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("").SetFontSize(13).SetBold().AddStyle(normalh));
                h5.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);

                table = new Table(1).UseAllAvailableWidth();
                table.AddCell(h3);
                table.AddCell(h4);
                table.AddCell(h5);
            }
        }


        [Authorize(Roles = "CARP_TM\\CARP_TM_WRITE")]
        [HttpPost]
        public async Task<IActionResult>  desfasurator_nou(string mod_reducere , string nr_nota_contabila , string data_nota_contabila , string credit , string nr_carnet_change_d )
        {
            //10 sept 2024 ok tbl

            int tip_modificare = 0;
           
            if (mod_reducere == "1") tip_modificare = 1;
            if (mod_reducere == "2") tip_modificare = 2;

            decimal soldimp_get_dob = 0;
            DateTime data_nota_contabila_OK;
            int test_data_now = 0;

            decimal sold_RA_repar001 = 0;
            decimal sold_imprumut_show_change_d = 0;

            double soldimp_desf_modificari = 0;
            DateTime data_desf_modificari = Convert.ToDateTime("12/31/2022");
            int nr_rate_desf_modificari = 0;

            int nr_nota_contabila_OK = 0;

            int i = 0;
            int nr_rate_modificare = 0;

            double get_dob_change_desf = 0;
            double dobanda_change_d = 0;


            decimal credit_OK = 0;

            double sold_impr_change_desf = 0;
            double rata_change_desf = 0;

            int nr_carnet_chage_d_OK = 0;
            int max_nr_desf = 0;
            int max_nr_rata = 0;
            int id_impr_desf_modif = 1;
            int test_now = 0;

            int nr_desfasurator_old = 0;

            int nrr_change_d = 0;
            int nrrate_change_d = 0;

            int nr_first_rata = 0;
            double total_total = 0, total_dobanda = 0;
            decimal total_rata_de_plata = 0;

            int nrrate_show_change_d = 0;


            decimal total_dobanda_ramasa_veche = 0;
            decimal total_dobanda_old = 0;

            decimal suma_rj = 0;

           
            DateTime now_change_d = DateTime.Now;

            int nr_lastD = 0;
            nr_lastD = 0;
            if ((DateTime.Now.Month == 11) || (DateTime.Now.Month == 2) || (DateTime.Now.Month == 4) || (DateTime.Now.Month == 6) || (DateTime.Now.Month == 9))
            {
                now_change_d = DateTime.Now.AddMonths(1);
                nr_lastD = 1;
            }


            int nrdaymonth = DateTime.DaysInMonth(now_change_d.Year, now_change_d.Month);
            int nrdaydiff = nrdaymonth - now_change_d.Day;

            now_change_d = now_change_d.AddDays(nrdaydiff);




            

            if (int.TryParse(nr_nota_contabila, out nr_nota_contabila_OK) == false)
            {
                ViewData["Error"] = "Introduceti o valoare numerica pentru nr nota contabila";
                return View("Error");
            }

            if (int.TryParse(nr_nota_contabila, out nr_nota_contabila_OK) == false)
            {
                ViewData["Error"] = "Introduceti o valoare numerica pentru nr nota contabila";
                return View("Error");
            }


            if (DateTime.TryParse(data_nota_contabila, out data_nota_contabila_OK) == false)
            {
                ViewData["Error"] = "Introduceti data";
                return View("Error");
            }

            test_data_now = 0;

            if (data_nota_contabila_OK.Month != DateTime.Now.Month) test_data_now = 1;
            if (data_nota_contabila_OK.Day != DateTime.Now.Day) test_data_now = 1;
            if (data_nota_contabila_OK.Year != DateTime.Now.Year) test_data_now = 1;

            if (test_data_now == 1)
            {
                ViewData["Error"] = "Data este diferita de data curenta";
                return View("Error");
            }



            if (int.TryParse(nr_carnet_change_d, out nr_carnet_chage_d_OK) == false)
            {
                ViewData["Error"] = "Introduceti o valoare numerica pentru nr carnet";
                return View("Error");
            }


            if (decimal.TryParse(credit, out credit_OK) == false)
            {
                ViewData["Error"] = "Introduceti o valoare numerica pentru credit";
                return View("Error");
            }
           

            if (nr_carnet_chage_d_OK != 99999)
            {

                var pens_change_desf =await _context.Pensionars
                    .Where(pcd => pcd.nrcarnet == nr_carnet_chage_d_OK).SingleOrDefaultAsync();



                if (pens_change_desf == null)
                {
                    ViewData["Error"] = "Nu exista acest nr de carnet";
                    return View("Error");
                }


                if (pens_change_desf.soldimp <= 0)
                {
                    ViewData["Error"] = "Nu exista sold imprumut  pentru acest nr de carnet";
                    return View("Error");
                }



                if (pens_change_desf.credit_imprumut < credit_OK)
                {
                    ViewData["Error"]  = "Ati introdus un credit prea mare pentru acest nr de carnet";
                    return View("Error");
                }


                var cek_exista_desf =await _context.desfasurator_rate_modificari.Where(drm => (drm.nrcarnet == nr_carnet_chage_d_OK) && (drm.data_modificare.Month == DateTime.Now.Month) && (drm.data_modificare.Year == DateTime.Now.Year)).ToListAsync();

                if (cek_exista_desf.Count() >= 1)
                {
                    ViewData["Error"] = "A mai fost creat un desfasurator in aceasta luna";
                    return View("Error");
                }



                var impr_2022 =await _context.imprumuturis.Where(idc => idc.nrcarnet == 99999).SingleOrDefaultAsync();


                var desfasurator_rate_now =await _context.desfasurator_rate
                 .Where(drn => (drn.nrcarnet == nr_carnet_chage_d_OK) && (drn.data_rata.Month == DateTime.Now.Month) && (drn.data_rata.Year == DateTime.Now.Year)).SingleOrDefaultAsync();


                var checkCh_dm =await _context.Chitantas
                   .Where(ccdm => (ccdm.nrcarnet == nr_carnet_chage_d_OK) && (ccdm.rata > 0) && (ccdm.data.Month == DateTime.Now.Month) && (ccdm.data.Year == DateTime.Now.Year)).ToListAsync();


                if (checkCh_dm.Count() == 0)
                {

                    if (credit_OK < desfasurator_rate_now.rata_de_plata + desfasurator_rate_now.dobanda + desfasurator_rate_now.rata_de_plata + 10)
                    {
                        ViewData["Error"] = "Credit insuficient pentru plata anticipata";
                        return View("Error");
                    }

                }


                if (credit_OK < desfasurator_rate_now.rata_de_plata + 10)
                {
                    ViewData["Error"] = "Credit insuficient";
                    return View("Error");
                }


                var exista_nc =await _context.Chitantas.Where(enc => ((enc.nrch == nr_nota_contabila_OK) && (enc.tip_document != 0))).ToListAsync();

                if (exista_nc.Count() >= 1)
                {
                    ViewData["Error"] = "Mai exista acest nr de nota contabila";
                    return View("Error");
                }


                //luna now plata
                if (checkCh_dm.Count() == 0)
                {
                    ViewData["Error"] = "Nu ati platit rata in aceasta luna";
                    return View("Error");
                }

                //insert nota in ch table

                _context.Chitantas.Add(new Chitanta
                {
                    nrch = nr_nota_contabila_OK,
                    data = DateTime.Now,
                    nrcarnet = pens_change_desf.nrcarnet,
                    cotizatie = 0,
                    ajdeces = 0,
                    lunaajdeces = pens_change_desf.lunaajdeces,
                    rata_de_plata = 0,
                    rata = 0,
                    dobanda = 0,
                    credit_imprumut = credit_OK,
                    taxainscr = 0,
                    total = credit_OK,
                    nume = pens_change_desf.nume,
                    id_utilizator = 0,
                    analitic = false,
                    tip_document = 4,
                    tip_operatie = "iesire",
                    serie= "CAR"
                });
                await _context.SaveChangesAsync();

                var getIDChCD =await _context.Chitantas
                                .Where(gicd => ((gicd.nrch == nr_nota_contabila_OK) && (gicd.tip_document == 4))).SingleOrDefaultAsync();


                //pa092024         


                //end luna now plata



                //introducere nota in RC


                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = DateTime.Now,
                    id_document = getIDChCD.idchitanta,
                    nr_document = nr_nota_contabila_OK.ToString(),
                    id_CS_debitor = 106,
                    id_CA_debitor = Int32.Parse("4622" + pens_change_desf.nrcarnet.ToString()),
                    id_CS_creditor = 103,
                    id_CA_creditor = Int32.Parse("2678" + pens_change_desf.nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = pens_change_desf.credit_imprumut,
                    SI_CS_credit = 0,
                    SI_CA_credit = pens_change_desf.soldimp,
                    debit = credit_OK,
                    credit = credit_OK,
                    SF_CS_debit = 0,
                    SF_CA_debit = pens_change_desf.credit_imprumut - credit_OK,
                    SF_CS_credit = 0,
                    SF_CA_credit = pens_change_desf.soldimp - credit_OK,
                    tip_document = 11,
                    sortare = "H",
                });

                pens_change_desf.credit_imprumut = pens_change_desf.credit_imprumut - credit_OK;
                pens_change_desf.soldimp = pens_change_desf.soldimp - credit_OK;
                pens_change_desf.restimp = pens_change_desf.restimp + credit_OK;

               await _context.SaveChangesAsync();


                //end introducere nota in RC


                sold_impr_change_desf = decimal.ToDouble(pens_change_desf.soldimp);
                sold_RA_repar001 = pens_change_desf.soldimp;

                //pa test practic

                var modificare_desfasurator = _context.desfasurator_rate
                    .Where(md => ((md.data_rata > DateTime.Now) && (md.nrcarnet == nr_carnet_chage_d_OK))).OrderBy(md => md.data_rata).ToList();

                total_dobanda_old = 0;

                int contor = 0;

                foreach (desfasurator_rate dr in modificare_desfasurator)
                {


                    if (i == 1)
                    {
                        id_impr_desf_modif = dr.id_imprumut;
                        nr_desfasurator_old = dr.nr_desfasurator;
                        nr_first_rata = dr.nr_rata;

                    }


                    test_now = 0;

                    if ((dr.data_rata.Month == DateTime.Now.Month) && (dr.data_rata.Year == DateTime.Now.Year))
                    {
                        test_now = 1;
                        i = i + 1;
                        nr_first_rata = dr.nr_rata + 1;
                        contor = 1;
                    }


                    if (test_now == 0)
                    {

                        if (credit_OK >= dr.rata_de_plata)
                        {
                            i = i + 1;
                            credit_OK = credit_OK - dr.rata_de_plata;

                        }

                        //am bagat this dec 2023
                        total_dobanda_old = total_dobanda_old + dr.dobanda;

                        _context.desfasurator_rate.Remove(dr);
                        _context.SaveChanges();

                    }
                }

                if (mod_reducere == "1") nrrate_change_d = modificare_desfasurator.Count() - i;
                if (mod_reducere == "2") nrrate_change_d = modificare_desfasurator.Count() - contor;

                nrrate_show_change_d = nrrate_change_d;

                var impr_change_desf = _context.imprumuturis
                    .Where(icd => icd.idimprum == id_impr_desf_modif).SingleOrDefault();



                if (impr_change_desf.data_imprumut.Year < 2023) soldimp_get_dob = pens_change_desf.soldimp2016;
                if (impr_change_desf.data_imprumut.Year >= 2023) soldimp_get_dob = impr_change_desf.valoare_imprumut;



                if (soldimp_get_dob <= 1000) get_dob_change_desf = 0.05;

                if ((soldimp_get_dob > 1000) && (soldimp_get_dob <= 3000)) get_dob_change_desf = 0.07;

                if ((soldimp_get_dob > 3000) && (soldimp_get_dob <= 10000)) get_dob_change_desf = 0.075;

                if ((soldimp_get_dob > 10000) && (soldimp_get_dob <= 25000)) get_dob_change_desf = 0.095;

                if ((soldimp_get_dob > 25000) && (soldimp_get_dob <= 35000)) get_dob_change_desf = 0.105;

                if ((soldimp_get_dob > 35000) && (soldimp_get_dob <= 50000)) get_dob_change_desf = 0.115;

                if (soldimp_get_dob == 70000) get_dob_change_desf = 0.12;



                if (pens_change_desf.nume.Substring(0, 6) == "C.A.R.") get_dob_change_desf = 0.05;


                _context.desfasurator_rate_modificari.Add(new desfasurator_rate_modificari
                {
                    id_imprumut = id_impr_desf_modif,
                    nrcarnet = nr_carnet_chage_d_OK,
                    nr_desfasurator = nr_desfasurator_old + 1,
                    tip_modificare = tip_modificare,
                    data_modificare = DateTime.Now,
                    sold_imprumut_modificat = (decimal)sold_impr_change_desf,
                    nr_rate = nrrate_change_d,
                    dobanda_imprumut = (decimal)get_dob_change_desf,

                });


                await _context.SaveChangesAsync();


                if (nr_desfasurator_old == 0)
                {
                    if (impr_change_desf.data_imprumut.Year < 2023)
                    {
                        impr_2022 = _context.imprumuturis.Where(idc => idc.nrcarnet == pens_change_desf.nrcarnet).SingleOrDefault();
                        soldimp_desf_modificari = (double)impr_2022.valoare_imprumut;
                    }

                    if (impr_change_desf.data_imprumut.Year >= 2023) soldimp_desf_modificari = (double)impr_change_desf.valoare_imprumut;

                    if (impr_change_desf.data_imprumut.Year >= 2023) data_desf_modificari = impr_change_desf.data_imprumut;

                    nr_rate_desf_modificari = modificare_desfasurator.Count() - contor + nr_first_rata - 1;

                    _context.desfasurator_rate_modificari.Add(new desfasurator_rate_modificari
                    {

                        id_imprumut = id_impr_desf_modif,
                        nrcarnet = nr_carnet_chage_d_OK,
                        nr_desfasurator = 0,
                        tip_modificare = 2,
                        data_modificare = (DateTime)data_desf_modificari,
                        sold_imprumut_modificat = (decimal)soldimp_desf_modificari,
                        dobanda_imprumut = (decimal)get_dob_change_desf,
                        nr_rate = nr_rate_desf_modificari

                    });

                    await _context.SaveChangesAsync();
                }



                //impr new start


                rata_change_desf = Financial.Pmt(get_dob_change_desf / 12, nrrate_change_d, -sold_impr_change_desf, 0, PaymentDue.EndOfPeriod);


                rata_change_desf = round_value(rata_change_desf);


                nrr_change_d = 0;


                total_rata_de_plata = 0;
                total_dobanda = 0;

                while (nrrate_change_d > 0)
                {
                    dobanda_change_d = (sold_impr_change_desf * get_dob_change_desf * 30) / 360;
                    dobanda_change_d = round_value(dobanda_change_d);

                    nrr_change_d = nrr_change_d + 1;
                    nrrate_change_d = nrrate_change_d - 1;


                    if ((rata_change_desf - dobanda_change_d) > sold_impr_change_desf)
                    {
                        rata_change_desf = dobanda_change_d + sold_impr_change_desf;
                        sold_impr_change_desf = 0;
                    };

                    if ((rata_change_desf - dobanda_change_d) <= sold_impr_change_desf)
                        sold_impr_change_desf = sold_impr_change_desf - (rata_change_desf - dobanda_change_d);


                    if ((nrrate_change_d == 0) && (sold_impr_change_desf > 0))
                    {
                        rata_change_desf = rata_change_desf + sold_impr_change_desf;
                        sold_impr_change_desf = 0;
                    }


                    _context.desfasurator_rate.Add(new desfasurator_rate
                    {
                        id_imprumut = id_impr_desf_modif,
                        nrcarnet = nr_carnet_chage_d_OK,
                        nr_rata = (nr_first_rata - 1) + nrr_change_d,
                        nr_rata_noua = nrr_change_d,
                        data_rata = now_change_d.AddMonths(nrr_change_d - nr_lastD),
                        sold_imp = (decimal)sold_impr_change_desf,
                        rata_de_plata = (decimal)(rata_change_desf - dobanda_change_d),
                        rata_platita = 0,
                        rata_credit = 0,
                        dobanda = (decimal)dobanda_change_d,
                        total = (decimal)rata_change_desf,
                        nr_desfasurator = nr_desfasurator_old + 1
                    });

                    await _context.SaveChangesAsync();

                    total_dobanda = total_dobanda + dobanda_change_d;
                    total_rata_de_plata = total_rata_de_plata + (decimal)(rata_change_desf - dobanda_change_d);
                    // total_total = total_total + rata_change_desf;

                }

                //impr new stop


                suma_rj = total_dobanda_old - (decimal)total_dobanda;

                //de aici dec 2023
                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = DateTime.Now,
                    id_document = getIDChCD.idchitanta,
                    nr_document = nr_nota_contabila_OK.ToString(),
                    id_CS_debitor = 109,
                    id_CA_debitor = Int32.Parse("6791" + pens_change_desf.nrcarnet.ToString()),
                    id_CS_creditor = 53,
                    id_CA_creditor = 103,
                    explicatie_nr_cont_analitic = "RA",
                    SI_CS_debit = 0,
                    SI_CA_debit = pens_change_desf.solddobanda,
                    SI_CS_credit = 0,
                    SI_CA_credit = 0,
                    debit = -suma_rj,
                    credit = -suma_rj,
                    SF_CS_debit = 0,
                    SF_CA_debit = (decimal)total_dobanda,
                    SF_CS_credit = 0,
                    SF_CA_credit = 0,
                    tip_document = 11,
                    sortare = "H"

                });


                pens_change_desf.solddobanda = (Decimal)total_dobanda;
                await _context.SaveChangesAsync();


                corectie_001_RA(pens_change_desf.nrcarnet, sold_RA_repar001, total_rata_de_plata);


            }




            return RedirectToAction("afisare_grafic_rambursare_nou", new { nr_desfasurator = nr_desfasurator_old + 1, idimpr_show_change_d = id_impr_desf_modif, nrcarnet_show_change_d = nr_carnet_chage_d_OK });


            return View();


        }

        [Authorize(Roles = "CARP_TM\\CARP_TM_WRITE")]
        public async Task<ActionResult> desfasurator_nou()
        {
            //10 sept 2024 ok tbl          

            ViewData["dataRA"] = DateTime.Now;
            return View();


        }

        public int corectie_001_RA(int nrcarnet_RA_repar001, decimal sold_RA_repar001, decimal total_RP_RA_repar001)
        {
            //09092024-TBL
            decimal dif_RP_sold_RA = 0;
            decimal dif_RP_sold_RA_dbl = 0;

            string date_start_RA_str;
            DateTime date_start_RA;

            date_start_RA_str = DateTime.Now.Year.ToString() + "/" + (DateTime.Now.Month + 1).ToString() + "/1";

            if (DateTime.Now.Month == 12)
                date_start_RA_str = (DateTime.Now.Year + 1).ToString() + "/" + ("1").ToString() + " / 1";

            date_start_RA = Convert.ToDateTime(date_start_RA_str);

            var desfRepar001RA = _context.desfasurator_rate.Where(dr01RA => (dr01RA.nrcarnet == nrcarnet_RA_repar001) && (dr01RA.data_rata > date_start_RA)).OrderByDescending(dr01RA => dr01RA.data_rata); ;

            dif_RP_sold_RA = desfRepar001RA.Sum(dr01RA => dr01RA.rata_de_plata);
            dif_RP_sold_RA = sold_RA_repar001 - dif_RP_sold_RA;


            if (dif_RP_sold_RA == 0.01M)
            {
                desfRepar001RA.First().rata_de_plata = desfRepar001RA.First().rata_de_plata + 0.01M;
                desfRepar001RA.First().total = desfRepar001RA.First().total + 0.01M;
                _context.SaveChanges();
            }


            if (dif_RP_sold_RA == -0.01M)
            {
                desfRepar001RA.First().rata_de_plata = desfRepar001RA.First().rata_de_plata - 0.01M;
                desfRepar001RA.First().total = desfRepar001RA.First().total - 0.01M;
                _context.SaveChanges();
            }


            //dif_RP_sold_RA = sold_RA_repar001 - total_RP_RA_repar001;
            //dif_RP_sold_RA_dbl = dif_RP_sold_RA;
            // dif_RP_sold_RA_dbl = round_value(dif_RP_sold_RA_dbl);deci
            //
            // dif_RP_sold_RA_dbl = round_value_decimal(dif_RP_sold_RA_dbl);




            return 1;
        }



        public ActionResult simulare_desfasurator_imprumut_nou(string valoare_imprumut = "0", int nr_carnet_ads = 99999)
        {
            DateTime data_scadenta_d;
            int nrrate_d = 0, nr_rate_total = 0;
            double rata = 0, rata_OK = 0, rataN = 0, rata_OKN = 0;
            DateTime data_imp_d;
            double sold_imp_d, dobanda_d = 0, sold_imp_OK = 0;
            int id_imprum = 0;
            int nrr = 0;
            decimal getdob = 0;
            string pdfDRS = "";
            double valoare_imprumut_OK = 0;
            double total_dobanda = 0;
            double total_total = 0;
            double total_rata = 0;

            //ViewBag.nrcarnet_ads = nr_carnet_ads;
            ViewData["nrcarnet_ads"] = nr_carnet_ads;
            int nr_rate_d_1 = 0;

            DateTime now = DateTime.Now;

            int nrdaymonth = DateTime.DaysInMonth(now.Year, now.Month);
            int nrdaydiff = nrdaymonth - now.Day;

            now = now.AddDays(nrdaydiff);

            var desfasurator_rate_simulator = new List<desfasurator_rate_simulator>();

            data_imp_d = DateTime.Now;

            if (double.TryParse(valoare_imprumut, out valoare_imprumut_OK) == false)
            {
                ViewBag.msgerr = "Introduceti o valoare numerica pentru valoare imprumut'";
                return View("Error");
            }

            if (valoare_imprumut_OK < 0)
            {
                ViewBag.msgerr = "valoare imprumut negativa";
                return View("Error");
            }


            sold_imp_d = valoare_imprumut_OK;

            if (valoare_imprumut_OK == 0) sold_imp_d = 100;



            if (valoare_imprumut_OK <= 1000) nrrate_d = 12;

            if ((valoare_imprumut_OK > 1000) && (valoare_imprumut_OK <= 3000)) nrrate_d = 24;

            if ((valoare_imprumut_OK > 3000) && (valoare_imprumut_OK <= 10000)) nrrate_d = 36;

            if (valoare_imprumut_OK > 10000) nrrate_d = 60;


            if (sold_imp_d <= 1000)
            {
                rata = Financial.Pmt(0.050 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.05M;
            }


            if ((sold_imp_d > 1000) && (sold_imp_d <= 3000))
            {
                rata = Financial.Pmt(0.06 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.06M;
            }


            if ((sold_imp_d > 3000) && (sold_imp_d <= 10000))
            {
                rata = Financial.Pmt(0.075 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.075M;
            }

            if ((sold_imp_d > 10000) && (sold_imp_d <= 25000))
            {
                rata = Financial.Pmt(0.095 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.095M;
            }

            if ((sold_imp_d > 25000) && (sold_imp_d <= 35000))
            {
                rata = Financial.Pmt(0.105 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.105M;
            }


            if ((sold_imp_d > 35000) && (sold_imp_d <= 49000))
            {
                rata = Financial.Pmt(0.115 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.115M;
            }


            if (sold_imp_d == 70000)
            {
                rata = Financial.Pmt(0.12 / 12, nrrate_d, -sold_imp_d, 0, PaymentDue.EndOfPeriod);
                getdob = 0.12M;
            }


            rata = round_value(rata);

            nrr = 0;

            nr_rate_d_1 = nrrate_d;

            if (valoare_imprumut_OK != 0)
            {


                while (nrrate_d > 0)
                {
                    dobanda_d = (sold_imp_d * decimal.ToDouble(getdob) * 30) / 360;
                    dobanda_d = round_value(dobanda_d);

                    total_dobanda = total_dobanda + dobanda_d;

                    nrr = nrr + 1;
                    nrrate_d = nrrate_d - 1;

                    if ((rata - dobanda_d) > sold_imp_d)
                    {
                        rata = dobanda_d + sold_imp_d;
                        sold_imp_d = 0;
                    };

                    if ((rata - dobanda_d) <= sold_imp_d)
                        sold_imp_d = sold_imp_d - (rata - dobanda_d);

                    if ((nrrate_d == 0) && (sold_imp_d > 0))
                    {
                        rata = rata + sold_imp_d;
                        sold_imp_d = 0;
                    }


                    //pa cek code, 2 pb zi rata 31 si prag 1000

                    desfasurator_rate_simulator.Add(new desfasurator_rate_simulator
                    {


                        nr_rata = nrr,
                        data_rata = now.AddMonths(nrr),
                        sold_imp = (decimal)sold_imp_d,
                        rata_de_plata = (decimal)(rata - dobanda_d),
                        dobanda = (decimal)dobanda_d,
                        total = (decimal)rata
                    });

                    _context.SaveChanges();

                    total_rata = total_rata + (rata - dobanda_d);
                    total_total = total_total + rata;                    
                }

            }
                                             

            @ViewBag.nrrate_ads = nr_rate_d_1;

            if (valoare_imprumut_OK == 0) @ViewBag.nrrate_ads = 0;
            @ViewBag.sold_impr_ads = valoare_imprumut_OK.ToString("N2");

            @ViewBag.nrcarnet_ads = nr_carnet_ads;

            ViewBag.total_dobandaS = total_dobanda.ToString("N2");
            ViewBag.total_rataS = total_rata.ToString("N2");
            ViewBag.total_totalS = total_total.ToString("N2");

            return View(desfasurator_rate_simulator);

        }

        public async Task<IActionResult> generare_contract(int nrcarnet_gd, decimal valoare_imprumut_gd)
        {

         var pens_gc = await _context.Pensionars.Where(pgc => pgc.nrcarnet == nrcarnet_gd).SingleOrDefaultAsync();

         MemoryStream ms = new MemoryStream();
         PdfWriter writer = new PdfWriter(ms);
         PdfDocument pdfDoc = new PdfDocument(writer);
         Document document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A4, false);
         writer.SetCloseStream(false);                     

         document.SetMargins(50f, 20f, 50f, 20f);

         string webRootPath = System.IO.Path.Combine(_env.WebRootPath, "PDF", "Fonts", "times.ttf");
          //  string webRootPath = _env.WebRootPath.ToString() + "\\PDF\\Fonts\\times.ttf";
         PdfFont font = PdfFontFactory.CreateFont(webRootPath, PdfEncodings.IDENTITY_H);

         document.SetFont(font);

            // Titlu
        document.Add(new Paragraph("CASA DE AJUTOR RECIPROC A PENSIONARILOR").SetTextAlignment(TextAlignment.CENTER).SetFontSize(14).SetBold());
        
        document.Add(new Paragraph("TURNU MĂGURELE").SetTextAlignment(TextAlignment.CENTER).SetFontSize(14).SetBold());
        document.Add(new Paragraph("\nCONTRACT DE ÎMPRUMUT").SetTextAlignment(TextAlignment.CENTER).SetFontSize(16).SetBold());
        document.Add(new Paragraph("Nr. _________ din data de ____________\n\n").SetFirstLineIndent(20)); 

        // Părțile contractante    
        document.Add(new Paragraph("Între").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
        document.Add(new Paragraph("C.A.R.P. Turnu Măgurele, cu sediul în strada Cetatea Turnu, nr. 8, localitatea Turnu Măgurele, județul Teleorman, constituită în baza Legii nr. 540/2002, cu modificările și completările ulterioare și înregistrată la Judecătoria Turnu Măgurele, prin Hotărârea judecătorească nr. 3168 din 17.10.2006, cod fiscal 7230589, " +
           "reprezentată legal prin HODINĂ VIOREL, Președinte și Contabil Șef, denumită în prezentul contract C.A.R.P. Turnu Măgurele,\n ").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
         document.Add(new Paragraph("" +  ""));
         document.Add(new Paragraph("și").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
         document.Add(new Paragraph(" Dl.(D-na) " + pens_gc.nume.Trim() + ", membru al C.A.R. PENSIONARI Turnu Măgurele, având nr. fișă " + pens_gc.nrcarnet + ", fiul (fiica) lui _______________________ și al _______________________, născut(ă) la data de ___________, " +
           // "Dl.(D-na) ____________________________, membru al C.A.R. PENSIONARI Turnu Măgurele, având nr. fișă _________, fiul (fiica) lui _______________________ și al _______________________, născut(ă) la data de ___________, " +
           "în localitatea _______________________, județul ________________________, cu domiciliul în localitatea " + pens_gc.localitate.Trim() + ", județul " + pens_gc.judet.Trim() + ", strada " + pens_gc.strada.Trim() + ", nr. " + pens_gc.nr.Trim() + " bloc " + pens_gc.bloc.Trim() + ", scara  ___, et. __, ap. " + pens_gc.ap + ", posesor(oare) al B.I.(C.I.), " +
           "seria ____, nr ___________, eliberat(ă) de ______________________, la data de __________________, CNP " + pens_gc.cnp.Trim() + ", pensionar(ă) cu dosarul nr. ___________/ salariat(ă) la ____________________________, conform adeverinţei nr. __________________, telefon serviciu/telefon acasă ______________________, " +
           "telefon mobil " + pens_gc.telefon + ", e-mail " + pens_gc.email + ", denumit în continuare ÎMPRUMUTAT.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));

          document.Add(new Paragraph("Împrumutatul recunoaște că a fost informat de condițiile de creditare ale C.A.R.P. Turnu Măgurele " +
            "prin intermediul formularului INFORMAȚII STANDARD LA NIVEL EUROPEAN PRIVIND CREDITUL PENTRU CONSUMATORI.\n" +
            "În baza cererii de împrumut nr. ______________, aprobată, se încheie prezentul contract de împrumut " +
            "în condițiile stipulate în contract și asupra cărora părțile au convenit de comun acord.\n\n").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
                        
          //art 1
          document.Add(new Paragraph("Art. 1 Obiectul contractului").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20).SetFontSize(14).SetUnderline());           
          document.Add(new Paragraph("1.1 Tipul de împrumut acordat membrului: Împrumut tradițional.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("1.2 C.A.R.P. Turnu Măgurele acordă ÎMPRUMUTATULUI un împrumut în sumă de ______________ ei adică (în litere) (_____________________________________ ) lei pe termen de ______ luni.\n\n").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));

          //art 2
          document.Add(new Paragraph("Art. 2 Eliberarea împrumutului").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20).SetFontSize(14).SetUnderline());           
          document.Add(new Paragraph("2.1 Împrumutul se pune la dispoziția ÎMPRUMUTATULUI numai după perfectarea contractelor de garanție și se plătește astfel: integral la data de __________").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("2.2 Împrumutul se acordă prin una din formele de la pct. 2.1. prin numerar la casierie sau virament în cont nr. ______________________\n\n").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));

          //art 3
          document.Add(new Paragraph("Art. 3 Costul total al împrumutului, comisioane, rata dobânzii").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20).SetFontSize(14).SetUnderline());
          document.Add(new Paragraph("3.1 Împrumutul se acordă cu o dobandă calculată la sold și se achită conform graficului.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("3.2 Rata dobânzii este fixă și se calculează de la data acordării împrumutului, inclusiv, până la data rambursarii integrale a acestuia, exclusiv.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));

          document.Add(new Paragraph("3.3 Calcularea dobânzii lunare datorate se efectuează astfel:").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("Dobânda datorată este:\n").Add(new Text("Sold Împrumut * Rata anuală dobândă.(%) * 30\n").SetUnderline()).Add("360\n").SetTextAlignment(TextAlignment.CENTER));

          document.Add(new Paragraph("3.4. Calcularea dobânzii lunare datorate se efectuează astfel:\n").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20)
                    .Add(new Tab()).Add("- ").Add("împrumutul in valoare de ____________ lei \n")
                    .Add(new Tab()).Add("- ").Add("dobânda totală în valoare de ____________ leireprezintă __ % pe an din sumaîmprumutată.\n"));

          document.Add(new Paragraph("3.5.Dobânda anuală efectivă(DAE),la data semnării prezentului contract, este de _______ % pe an. Costul total al împrumutului cuprinde numai valoarea dobânzii calculate. În dobânda anuală efectivă (DAE)" +
                                    " este inclusa numai dobânda anuală, diferența între rata dobânzii indicate la art.3.1 și DAE nu este un cost suplimentar, find determinată de formula diferită de calcul, prevazută de lege.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));               
          document.Add(new Paragraph("3.6.Nerambursarea împrumutului angajat la termenul stabilit în contract atrage după sine plata de către ÎMPRUMUTAT de dobânzi penalizatoare, în cuantum de 0,01% pe zi, calculate la suma" +
                                     " restantă, la care se adaugă dobânda arătată la art. 3.4. Dobânzile penalizatoare se aplică după trei luni consecutive de neplată a ratelor integrale , dar nu mai mult de 6 luni.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));             
          document.Add(new Paragraph("3.7 Comisioane. Nu se percep comisioane la acordarea împrumutului\n\n").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));


          //art 4
          document.Add(new Paragraph("Art. 4 Rambursare împrumutului").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20).SetFontSize(14).SetUnderline());           
          document.Add(new Paragraph("4.1 ÎMPRUMUTATUL se obligă să restituie C.A.R.P. Turnu Măgurele împrumutul primit împreună cu dobânda aferentă într-un număr de ____ rate lunare, începând cu data de ____________Termenele de restituire a ratelor scadente plus dobânzile aferente sunt prevăzute în GRAFICUL DE RAMBURSARE, ANEXA la contract").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("4.2 Restituirea ratelor lunare scadente la termenele prevăzute se face prin: numerar la casierie sau prin virament (inclusiv prin debitare directă)").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("4.3 Sumele achitate în conturile bancare ale C.A.R.P. Turnu Măgurele vor fi înregistrate în contul ÎMPRUMUTATULUI pe baza extrasului de cont. Orice comisioane sau taxe aferente viramentelor în contul C.A.R.P. Turnu Măgurelepercepute de unitatea bancară a Casei de Ajutor Reciproc Turnu Măgurele, vor fi suportate de ÎMPRUMUTAT").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("4.4 La solicitarea ÎMPRUMUTATULUI, împrumutul poate fi rambursat înainte de scadență, dobânda calculându-se numai pe perioada de utilizare. Pentru membrii care plătesc anticipat se va recalcula dobânda la suma rămasă de rambursat sau se va reduce perioada de rambursare").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("4.5 În cazul în care drepturile bănești ce urmează a fi reținute și virate de către unitatea la care este angajat ÎMPRUMUTATUL(TERȚ POPRIT) nu acoperă integral nivelul ratei de rambursat, acesta se obligă să depună, în numerar la casieria C.A.R.P. Turnu Măgurele diferența respectivă. Nedepunerea diferenței constituie rată scadentă și nerambursată, aplicând dispozițiile art. 6.2 lit. c din prezentul contract.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("4.6 ÎMPRUMUTATUL este considerat de drept pus în întârziere la momentul împlinirii termenului stipulat în contract pentru executarea obligației.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("4.7 Orice sumă plătită către C.A.R.P. Turnu Măgurele de ÎMPRUMUTAT, indiferent de titlul și/sau specificația cu care acesta înțelege să efectueze plata, va stinge obligațiile scadente înainte sau la data creditării contului C.A.R.P. Turnu Măgurele în ordinea stabilită de lege, chiar dacă ÎMPRUMUTATUL a menționat pe documentul cu care efectuează plata că dorește să stingă altă datorie.\n\n").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));


          //art 5
          document.Add(new Paragraph("Art. 5 Garantarea împrumutului").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20).SetFontSize(14).SetUnderline());           
          document.Add(new Paragraph("5.1  La acordarea împrumutului și pe perioada derulării prezentului contract nu se percep costuri suplimentare.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("5.2  Costul Total al Împrumutului, la data semnării prezentului contract este în valoare de ___________ lei şi este compus din valoarea totală a dobânzilor. Valoarea totală a dobânzilor de _____________ lei este valabilă doar în cazul respectării scadenţelor şi sumelor prevăzute în Graficul de rambursare.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("5.3  Prezentul contract poate fi modificat și/sau completat numai cu acordul ambelor părți, prin acte adiţionale la prezentul contract.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("5.4  Modificarea dobânzii pe parcursul derulării prezentului contract se poate face numai în condiţiile legii.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("5.5  C.A.R. va informa ÎMPRUMUTATUL despre o asemenea modificare printr-o notificare scrisă cu cel puţin 30 de zile înainte de a opera modificarea, sau printr-o notificare prin e-mail, la adresele indicate de ÎMPRUMUTAT prin prezentul contract.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("5.6  În cazul în care ÎMPRUMUTATUL nu este de acord cu noua rată a dobânzii aplicabile, are la dispoziţie un termen de 15 zile de la primirea notificării pentru a denunţa contractul sau a continua executarea lui în noile condiţii.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("5.7  Pentru a-şi exercita dreptul de a denunţa contractul ÎMPRUMUTATUL va trebui să transmită o notificare scrisă, prin scrisoare recomandată, către C.A.R.P., în termen de 15 zile.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("5.8  La data acestei denunţări, toate obligaţiile ÎMPRUMUTATULUI vor deveni imediat scadente şi exigibile.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("5.9  Neprimirea niciunei obiecţiuni din partea ÎMPRUMUTATULUI în termenul de 15 zile constituie o acceptare a noii rate de dobândă din partea ÎMPRUMUTATULUI.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("5.10 În situaţia în care ÎMPRUMUTATUL nu informează C.A.R.P. despre schimbarea datelor sale de contact (adresă domiciliu, nr. de telefon, adresă e-mail), notificările/comunicările se vor considera comunicate la adresele declarate de ÎMPRUMUTAT prin prezentul contract, iar C.A.R.P. nu va putea fi ţinut răspunzător pentru orice eventual prejudiciu cauzat ÎMPRUMUTATULUI ca urmare a modificării ratei dobânzii.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("5.11 Alte elemente de cost care revin ÎMPRUMUTATULUI, care nu intră în calculul DAE, sunt cele legate de asigurări sau garanţii, respectiv înscrierea garanţiei reale mobiliare asupra soldurilor creditoare prezente şi viitoare aleconturilor curente ale ÎMPRUMUTATULUI / GIRANȚILOR la Arhiva Electronică de Garanţii Reale Mobiliare sau încheierea de către ÎMPRUMUTAT a unui contract de asigurare.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("5.12 Împrumutul în sumă de _____________lei și dobânda aferentă în sumă de _________ lei sunt garantate de ÎMPRUMUTAT prin: venituri realizate sub orice formă, fondul social și giranți").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));
          document.Add(new Paragraph("5.13 Fondul social al ÎMPRUMUTATULUI și respectiv al GIRANȚILOR nu se poate retrage de la C.A.R.P. Turnu Măgurele decât după rambursarea integrală a împrumutului, conform statutului și normelor interne de creditare ale C.A.R.P. Turnu Măgurele.\n\n").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));


        //art 6
         document.Add(new Paragraph("Art. 6 Obligații și drepturile părților").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20).SetFontSize(14).SetUnderline());
         document.Add(new Paragraph("6.1  ÎMPRUMUTATUL se obligă să restituie împrumutul și dobânzile calculate la termenele convenite, conformGRAFICULUI DE RAMBURSARE (ANEXA); să comunice în termen de cinci zile la C.A.R.P. Turnu Măgurele orice schimbare intervenităprivind domiciliul, locul de muncă, telefonul sau actul de identitate și să restituie C.A.R.P. cheltuielile efectuate pentru recuperarea împrumutului, dobânzilorneachitate la scadență.").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20));         
         document.Add(new Paragraph("6.2. C.A.R.P. Turnu Măgurele are dreptul:\n").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20)
                 .Add(new Tab()).Add("a) ").Add("să încaseze de la ÎMPRUMUTAT dobânzile aferente împrumutului acordat, în condițiile și cuantumurile și la termenele stabilite în prezentul contract;\n")
                 .Add(new Tab()).Add("a) ").Add("să încaseze de la ÎMPRUMUTAT dobânzile aferente împrumutului acordat, în condițiile și cuantumurile și la termenele stabilite în prezentul contract;\n")
                 .Add(new Tab()).Add("b) ").Add("să încaseze dobânzile și ratele restante scadente din fondul social al ÎMPRUMUTATULUI, în condițiile legii;\n")
                 .Add(new Tab()).Add("c) ").Add("să rezilieze unilateral contractul de împrumut după notificarea împrumutatului printr-o adresă scrisă, în situația în care ÎMPRUMUTATUL înregistrează debite restante, având scadența depășită cu o perioadă mai mare de 90 de zile calendaristice, diminuează ori anulează garanțiile acordate sau încalcă prevederile art. 5.2 din contract;\n")
                 .Add(new Tab()).Add("d) ").Add("rezilierea acționează de deplin drept fără a fi necesară punerea în întârziere a ÎMPRUMUTATULUI, fără intervenția instanței de judecată și fără orice altă formalitate prealabilă. La data rezilierii contractului, obligațiile de plată ale ÎMPRUMUTATULUI, incluzând, fără a se limita la întreaga valoare a împrumutului nerestituit, la care se adaugă dobânzile recalculate și dobânzile penalizatoare, devin exigibile. Asupra soldului împrumutului nerestituit se calculează dobânzi până la încasarea efectivă a acestuia de către C.A.R.P. Turnu Măgurele;\n")
                 .Add(new Tab()).Add("e) ").Add("să urmărească prin procedura executării silite pe ÎMPRUMUTAT și pe oricare dintre GIRANȚII săi, în vederea recuperării integrale a debitului, inclusiv cheltuielile de urmărire și executare silită."));
            //

         document.Add(new Paragraph("6.3. C.A.R.P. Turnu Măgurele se obligă:\n").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20)
                 .Add(new Tab()).Add("a) ").Add("să pună la dispoziția ÎMPRUMUTATULUI suma împrumutată în condițiile convenite, înconformitate cu prevederile art. 2 din prezentul Contract;\n")
                 .Add(new Tab()).Add("b) ").Add("să comunice ÎMPRUMUTATULUI, în termen de 5 zile de la înregistrarea solicitării,situația sumelor (reprezentând împrumut, dobânzi, etc.) rămase de restituit;\n")
                 .Add(new Tab()).Add("c) ").Add("să notifice în scris atât ÎMPRUMUTATUL cât și GIRANȚII cu privire la neîndeplinireaobligației de restituire a împrumutului și dobânzilor aferente și la faptul că s-a trecut la recuperarea acestora, în conformitate cu dispozițiile art. 8.\n"));

         document.Add(new Paragraph("6.4. ÎMPRUMUTATUL are dreptul:\n").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20)
                 .Add(new Tab()).Add("a) ").Add("să i se elibereze împrumutul, în condițiile convenite, în conformitate cu prevederile art. 2;\n")               
                 .Add(new Tab()).Add("b) ").Add("să ramburseze anticipat împrumutul și dobânzile aferente, calculate până la data restituirii.\n"));


         document.Add(new Paragraph("6.2. C.A.R.P. Turnu Măgurele are dreptul:\n").SetTextAlignment(TextAlignment.JUSTIFIED).SetFirstLineIndent(20)
                .Add(new Tab()).Add("1) ").Add("Împrumutatul are la dispoziție un termen de 14 zile calendaristice de la data semnăriicontractului în care se poate retrage din contract fără nici o justificare prin trimiterea către C.A.R.P. Turnu Măgurele a unei notificări în acest sens.\n")
                .Add(new Tab()).Add("2) ").Add("În cazul în care împrumutatul își exercită dreptul de retragere, acesta are următoarele obligații:\n")
                .Add(new Tab()).Add("a) ").Add("de a notifica C.A.R.P. Turnu Măgurele cu privire la retragerea din contract, în termenulmenționat la alin. 1 de mai sus, precizând data la care va efectua rambursarea împrumutului, dar nu mai târziu de 30 de zile calendaristice de la data expedierii notificării. Notificarea va fi făcută pe suport de hârtie și va fi transmisă C.A.R.P. Turnu Măgurele , la sediul unității unde împrumutatul a contractat creditul, prin scrisoare recomandată cu confirmare de primire, prin curier sau prin depunere personală. Exercitarea dreptului de retragere își produce efecte de la data expedierii notificării , respectiv data poștei(în cazul transmiterii prin poștă cu confirmare de primire sau prin curier), respectiv data înregistrării notificării la registratura C.A.R.P. Turnu Măgurele (în cazul depunerii personale).\n")
                .Add(new Tab()).Add("b) ").Add("de a plăti C.A.R.P. Turnu Măgurele până la data indicată de împrumutat în notificare,împrumutul și dobânda aferentă de la data la care creditul a fost pus la dispoziție până la data când creditul a fost rambursat;\n")
                .Add(new Tab()).Add("3) ").Add("Obligația de plată către C.A.R.P. Turnu Măgurele a creditului și a dobânzii menționate maisus la alin.2 lit.b trebuie să fie executată fără nici o intârziere nejustificată nu mai târziu de 90 zile calendaristice de la data transmiterii notificării de retragere către C.A.R.P. Turnu Măgurele.\n")
                .Add(new Tab()).Add("4) ").Add("La expirarea termenului de 90 zile calendaristice menționat la art. 3, C.A.R.P. TurnuMăgurele, va fi îndreptățită să perceapă rata de dobândă penalizatoare în cuantumul stabilit în rezentul contract și să inițieze toate demersurile pentru recuperarea integrală a creditului acordat, a dobânzii aferente, precum și a cheltuielilor de recuperare a acestor sume.\n\n"));



            document.Close();
            byte[] byteInfo = ms.ToArray();
            ms.Write(byteInfo, 0, byteInfo.Length);
            ms.Position = 0;
            FileStreamResult fileStreamResult = new FileStreamResult(ms, "application/pdf");
            return fileStreamResult;
        }

        private bool imprumuturiExists(int id)
        {
            return _context.imprumuturis.Any(e => e.idimprum == id);
        }
    }
}
