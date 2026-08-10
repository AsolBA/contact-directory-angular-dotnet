import { Component, OnInit } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ChartModule } from 'primeng/chart';
import 'chart.js/auto';

//backendden gelen tek bir log kaydının şekli 
interface AuditLog {
  id: number;
  userName: string;
  actionType: string;
  details: string;
  createdAt: string;
}
//grafik endpointinden gelen istatistik şekli
interface LogStat {
  userName: string;
  count: number;
}
//sol taraftaki admin chip'in şekli
interface AdminChip {
  userName: string;
  color: string;
}

@Component({
  selector: 'app-logs',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    TableModule,
    ChartModule
  ],
  templateUrl: './logs.html',
  styleUrl: './logs.css'
})
export class Logs implements OnInit {
  logs: AuditLog[] = [];//apiden gelen tüm sayfalar
  chartData: any;//grafiğin datası
  chartOptions: any;//grafiğin ayarları
  currentPage: number = 1;//şuan hangi sayfadayız
   pageSize: number = 50;//bir sayfadan kaç log çekilecek
  loginLogoutList: AuditLog[] = [];//sadece login/logout olanlar
  contactActionList: AuditLog[] = [];//sadece kişi crud olanlar
  adminList: AdminChip[] = [];// sol taraftaki admin list
  selectedAdmin: string | null = null; // şu an seçili olan admin adı
  pieChartData: any;                   // pasta grafiğin data'sı
  pieChartOptions: any;                // pasta grafiğin görünüm ayarları
  //toplam sayfa sayısı = toplamlog/pagesize sonucu yukarı yuvarlanıyor
  // İstatistik Kartları için değişkenler
  totalLogs: number = 0;
  uniqueAdmins: number = 0;
  private adminColors: string[] = [
  '#2563eb', '#0f766e', '#d97706', '#e11d48', '#7c3aed', '#0891b2'
  ];

  constructor(private readonly http: HttpClient, private readonly router: Router) {}

  ngOnInit(): void {
    this.fetchLogs();//sayfa açılınca tablolar
    this.fetchStats();//sayfa açılınca admin listesi + pasta grafiği
  }

  // Backend userName veya UserName gönderebilir ikisini de oku(camel veya pascal case gelebilir)
  private mapLog(l: any): AuditLog {
    return {                     
      id: l.id ?? l.Id,
      userName: l.userName ?? l.UserName,
      actionType: l.actionType ?? l.ActionType,
      details: l.details ?? l.Details,
      createdAt: l.createdAt ?? l.CreatedAt
    };
  }

    // 1. Log Tablosunu API'den Çekme
  fetchLogs(): void {
    this.http.get<any>(`http://localhost:5163/api/logs?page=${this.currentPage}&pageSize=${this.pageSize}`).subscribe({
      next: (res) => {
        // Backend logs / Logs farkını tolere et
        const rawLogs = res.logs ?? res.Logs ?? [];

        // Her kaydı tek formata çevir
        this.logs = rawLogs.map((l: any) => this.mapLog(l));

        // Toplam kayıt sayısı
        this.totalLogs = res.totalCount ?? res.TotalCount ?? 0;

        // Giriş-çıkış tablosu için filtre
        this.loginLogoutList = this.logs.filter(l => {
          const t = (l.actionType || '').toUpperCase();
          return t === 'LOGIN' || t === 'LOGOUT';
        });

        // Kişi işlemleri tablosu için filtre
        this.contactActionList = this.logs.filter(l => {
          const t = (l.actionType || '').toUpperCase();
          return t === 'CREATE_CONTACT' || t === 'UPDATE_CONTACT' || t === 'DELETE_CONTACT';
        });
      },
      error: (err) => console.error('Loglar çekilemedi:', err)
    });
  }

  // Admin listesini çeker + ilk admini otomatik seçer
  fetchStats(): void {
    this.http.get<any[]>('http://localhost:5163/api/logs/stats').subscribe({
      next: (stats) => {
        const normalized = (stats ?? []).map((s: any) => ({
          userName: s.userName ?? s.UserName,
          count: s.count ?? s.Count
        }));

        this.uniqueAdmins = normalized.length;

        // Sol listedeki admin chip'lerini doldur
        this.adminList = normalized.map((s: any) => ({
          userName: s.userName,
          color: this.getColorForAdmin(s.userName)
        }));

        // İlk admini otomatik seç
        if (this.adminList.length > 0) {
          this.selectAdmin(this.adminList[0].userName);
        }
      },
      error: (err: any) => console.error('İstatistikler çekilemedi:', err)
    });
  }

  // Admin chip'ine tıklanınca çalışır,selectedadmin güncellenir,pasta verisi yeniden açılır
  selectAdmin(userName: string): void {
    this.selectedAdmin = userName;
    this.fetchAdminPieStats(userName);
  }

  // Seçilen adminin işlem tiplerini al,etiketleri türkçeleştir pieChartData / pieChartOptions doldur.
  fetchAdminPieStats(userName: string): void {
    this.http.get<any[]>(`http://localhost:5163/api/logs/stats/${encodeURIComponent(userName)}`).subscribe({
      next: (stats) => {
        const normalized = (stats ?? []).map((s: any) => ({
          actionType: s.actionType ?? s.ActionType,
          count: s.count ?? s.Count
        }));

        const labels = normalized.map((s: any) => this.getActionLabel(s.actionType));
        const counts = normalized.map((s: any) => s.count);

        this.pieChartData = {
          labels: labels,
          datasets: [
            {
              data: counts,
              backgroundColor: ['#2563eb', '#0f766e', '#d97706', '#e11d48', '#7c3aed', '#0891b2']
            }
          ]
        };

        this.pieChartOptions = {
          responsive: true,
          maintainAspectRatio: false,
          plugins: {
            legend: {
              position: 'bottom',
              labels: {
                color: '#1e3a5f',
                font: { weight: '600' }
              }
            }
          }
        };
      },
      error: (err: any) => console.error('Admin pasta verisi çekilemedi:', err)
    });
  }

  // İngilizce actionType'ı Türkçe etikete çevirir
  getActionLabel(actionType: string): string {
    switch ((actionType || '').toUpperCase()) {
      case 'CREATE_CONTACT': return 'Kişi Ekleme';
      case 'UPDATE_CONTACT': return 'Kişi Güncelleme';
      case 'DELETE_CONTACT': return 'Kişi Silme';
      case 'LOGIN': return 'Giriş';
      case 'LOGOUT': return 'Çıkış';
      default: return actionType;
    }
  }

  // Admin adına sabit renk üretir
  getColorForAdmin(name: string): string {
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash += name.codePointAt(i) ?? 0;
    }
    return this.adminColors[hash % this.adminColors.length];
  }

  // Rehber listesine geri döner
  goBack(): void {
    this.router.navigate(['/contacts']);
  }

}